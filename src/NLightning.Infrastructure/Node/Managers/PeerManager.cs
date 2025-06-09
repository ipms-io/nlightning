using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NLightning.Infrastructure.Node.Managers;

using Application.Node.Interfaces;
using Domain.Crypto.ValueObjects;
using Domain.Exceptions;
using Domain.Node.Options;
using Interfaces;
using Protocol.Models;

/// <summary>
/// Service for managing peers.
/// </summary>
/// <remarks>
/// This class is used to manage peers in the network.
/// </remarks>
/// <seealso cref="IPeerManager" />
public sealed class PeerManager : IPeerManager
{
    private readonly ILogger<PeerManager> _logger;
    private readonly IOptions<NodeOptions> _nodeOptions;
    private readonly IPeerServiceFactory _peerServiceFactory;
    private readonly Dictionary<CompactPubKey, IPeerService> _peers = [];

    public PeerManager(ILogger<PeerManager> logger, IOptions<NodeOptions> nodeOptions,
                       IPeerServiceFactory peerServiceFactory)
    {
        _logger = logger;
        _nodeOptions = nodeOptions;
        _peerServiceFactory = peerServiceFactory;
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

        var peer = await _peerServiceFactory.CreateConnectedPeerAsync(peerAddress.PubKey, tcpClient);
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
        var peer = await _peerServiceFactory.CreateConnectingPeerAsync(tcpClient);
        peer.DisconnectEvent += (_, _) =>
        {
            _peers.Remove(peer.PeerPubKey);
            _logger.LogError("{Peer} disconnected", peer.PeerPubKey);
        };

        _peers.Add(peer.PeerPubKey, peer);
    }

    /// <inheritdoc />
    public void DisconnectPeer(CompactPubKey pubKey)
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