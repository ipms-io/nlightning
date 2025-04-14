using System.Net;
using System.Net.Sockets;

namespace NLightning.Bolts.BOLT1.Managers;

using Common.Interfaces;
using Common.Managers;
using Interfaces;
using Primitives;

/// <summary>
/// Service for managing peers.
/// </summary>
/// <remarks>
/// This class is used to manage peers in the network.
/// </remarks>
/// <seealso cref="IPeerManager" />
public sealed class PeerManager(ITransportServiceFactory transportServiceFactory,
                                IPingPongServiceFactory pingPongServiceFactory,
                                IMessageServiceFactory messageServiceFactory,
                                ILogger logger) : IPeerManager
{
    private readonly Dictionary<NBitcoin.PubKey, Peer> _peers = [];
    private readonly Dictionary<ChannelId, NBitcoin.PubKey> _channels = [];

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

        // Create and Initialize the transport service
        var transportService = transportServiceFactory
            .CreateTransportService(logger, true, SecureKeyManager.GetPrivateKeyBytes(), peerAddress.PubKey.ToBytes(), tcpClient);

        try
        {
            await transportService.InitializeAsync();
        }
        catch (Exception ex)
        {
            throw new ConnectionException($"Error connecting to peer {peerAddress.Host}:{peerAddress.Port}", ex);
        }

        var messageService = messageServiceFactory.CreateMessageService(transportService);
        var pingPongService = pingPongServiceFactory.CreatePingPongService();

        var peer = new Peer(messageService, pingPongService, logger, peerAddress, false);
        peer.DisconnectEvent += (sender, e) =>
        {
            _peers.Remove(peerAddress.PubKey);
            logger.Information(e, sender, "Peer {Peer} disconnected", peerAddress.PubKey);
        };

        _peers.Add(peerAddress.PubKey, peer);
    }

    /// <inheritdoc />
    /// <exception cref="ErrorException">Thrown when the connection to the peer fails.</exception>
    public async Task AcceptPeerAsync(TcpClient tcpClient)
    {
        // Get peer data
        var remoteEndPoint = (IPEndPoint)(tcpClient.Client.RemoteEndPoint
                                          ?? throw new Exception("Failed to get remote endpoint"));
        var ipAddress = remoteEndPoint.Address.ToString();
        var port = remoteEndPoint.Port;

        // Create and Initialize the transport service
        var key = SecureKeyManager.GetPrivateKey();
        var transportService = transportServiceFactory.CreateTransportService(logger, false, key.ToBytes(),
                                                                              key.PubKey.ToBytes(), tcpClient);
        await transportService.InitializeAsync();
        if (transportService.RemoteStaticPublicKey is null)
        {
            throw new ErrorException("Failed to get remote static public key");
        }
        var peerAddress = new PeerAddress(transportService.RemoteStaticPublicKey, ipAddress, port);

        var messageService = messageServiceFactory.CreateMessageService(transportService);
        var pingPongService = pingPongServiceFactory.CreatePingPongService();

        // Create the peer
        var peer = new Peer(messageService, pingPongService, logger, peerAddress, true);
        peer.DisconnectEvent += (sender, e) =>
        {
            _peers.Remove(peerAddress.PubKey);
            logger.Information(e, sender, "{Peer} disconnected", peerAddress.PubKey);
        };

        _peers.Add(peerAddress.PubKey, peer);
    }
}