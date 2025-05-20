using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace NLightning.Infrastructure.Node.Managers;

using Domain.Exceptions;
using Domain.Node.Options;
using Domain.ValueObjects;
using Infrastructure.Protocol.Models;
using Interfaces;
using Models;

/// <summary>
/// Service for managing peers.
/// </summary>
/// <remarks>
/// This class is used to manage peers in the network.
/// </remarks>
/// <seealso cref="IPeerManager" />
public sealed class PeerManager : IPeerManager
{
    private readonly Dictionary<ChannelId, PubKey> _channels = [];
    private readonly ILogger<PeerManager> _logger;
    private readonly IOptions<NodeOptions> _nodeOptions;
    private readonly IPeerFactory _peerFactory;
    private readonly Dictionary<PubKey, Peer> _peers = [];

    public PeerManager(ILogger<PeerManager> logger, IOptions<NodeOptions> nodeOptions, IPeerFactory peerFactory)
    {
        _logger = logger;
        _nodeOptions = nodeOptions;
        _peerFactory = peerFactory;
    }

    /// <inheritdoc />
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    public async Task ConnectToPeerAsync(PeerAddress peerAddress)
    {
        // Connect to the peer
        var tcpClient = new TcpClient();
        try
        {
            await tcpClient.ConnectAsync(peerAddress.Host, peerAddress.Port,
                                         new CancellationTokenSource(_nodeOptions.Value.NetworkTimeout).Token);
        }
        catch (OperationCanceledException)
        {
            throw new ConnectionException($"Timeout connecting to peer {peerAddress.Host}:{peerAddress.Port}");
        }
        catch (Exception e)
        {
            throw new ConnectionException($"Failed to connect to peer {peerAddress.Host}:{peerAddress.Port}", e);
        }

        var peer = await _peerFactory.CreateConnectedPeerAsync(peerAddress, tcpClient);
        peer.DisconnectEvent += (_, _) =>
        {
            _peers.Remove(peerAddress.PubKey);
            _logger.LogInformation("Peer {Peer} disconnected", peerAddress.PubKey);
        };

        _peers.Add(peerAddress.PubKey, peer);
    }

    /// <inheritdoc />
    /// <exception cref="ErrorException">Thrown when the connection to the peer fails.</exception>
    public async Task AcceptPeerAsync(TcpClient tcpClient)
    {
        // Create the peer
        var peer = await _peerFactory.CreateConnectingPeerAsync(tcpClient);
        peer.DisconnectEvent += (_, _) =>
        {
            _peers.Remove(peer.PeerAddress.PubKey);
            _logger.LogError("{Peer} disconnected", peer.PeerAddress.PubKey);
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
            _logger.LogWarning("Peer {Peer} not found", pubKey);
        }
    }
}