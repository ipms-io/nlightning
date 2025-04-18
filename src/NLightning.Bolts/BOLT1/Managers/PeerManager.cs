using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using NBitcoin;

namespace NLightning.Bolts.BOLT1.Managers;

using Common.Interfaces;
using Common.Managers;
using Common.Node;

/// <summary>
/// Service for managing peers.
/// </summary>
/// <remarks>
/// This class is used to manage peers in the network.
/// </remarks>
/// <seealso cref="IPeerManager" />
public sealed class PeerManager(IPeerFactory peerFactory, ILogger<PeerManager> logger) : IPeerManager
{
    private readonly Dictionary<PubKey, Peer> _peers = [];
    private readonly Dictionary<ChannelId, PubKey> _channels = [];

    /// <inheritdoc />
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    public async Task ConnectToPeerAsync(PeerAddress peerAddress)
    {
        // Connect to the peer
        var tcpClient = new TcpClient();
        try
        {
            await tcpClient.ConnectAsync(peerAddress.Host, peerAddress.Port,
                                         new CancellationTokenSource(ConfigManager.Instance.NetworkTimeout).Token);
        }
        catch (OperationCanceledException)
        {
            throw new ConnectionException($"Timeout connecting to peer {peerAddress.Host}:{peerAddress.Port}");
        }
        catch (Exception e)
        {
            throw new ConnectionException($"Failed to connect to peer {peerAddress.Host}:{peerAddress.Port}", e);
        }

        var peer = await peerFactory.CreateConnectedPeerAsync(peerAddress, tcpClient);
        peer.DisconnectEvent += (_, _) =>
        {
            _peers.Remove(peerAddress.PubKey);
            logger.LogInformation("Peer {Peer} disconnected", peerAddress.PubKey);
        };

        _peers.Add(peerAddress.PubKey, peer);
    }

    /// <inheritdoc />
    /// <exception cref="ErrorException">Thrown when the connection to the peer fails.</exception>
    public async Task AcceptPeerAsync(TcpClient tcpClient)
    {
        // Create the peer
        var peer = await peerFactory.CreateConnectingPeerAsync(tcpClient);
        peer.DisconnectEvent += (_, _) =>
        {
            _peers.Remove(peer.PeerAddress.PubKey);
            logger.LogError("{Peer} disconnected", peer.PeerAddress.PubKey);
        };

        _peers.Add(peer.PeerAddress.PubKey, peer);
    }

    /// <inheritdoc />
    public void DisconnectPeer(PubKey pubKey)
    {
        if (_peers.TryGetValue(pubKey, out var peer))
        {
            peer.Disconnect();
        }
        else
        {
            logger.LogWarning("Peer {Peer} not found", pubKey);
        }
    }
}