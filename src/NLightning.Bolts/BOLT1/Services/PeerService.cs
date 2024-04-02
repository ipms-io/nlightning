using System.Net;
using System.Net.Sockets;

namespace NLightning.Bolts.BOLT1.Services;

using BOLT1.Interfaces;
using BOLT1.Primitives;

public sealed class PeerService(NodeOptions nodeOptions, ITransportServiceFactory transportServiceFactory, IPingPongServiceFactory pingPongServiceFactory, IMessageServiceFactory messageServiceFactory) : IPeerService
{
    private readonly NodeOptions _nodeOptions = nodeOptions;
    private readonly ITransportServiceFactory _transportServiceFactory = transportServiceFactory;
    private readonly IPingPongServiceFactory _pingPongServiceFactory = pingPongServiceFactory;
    private readonly IMessageServiceFactory _messageServiceFactory = messageServiceFactory;
    private readonly Dictionary<NBitcoin.PubKey, Peer> _peers = [];
    private readonly Dictionary<ChannelId, NBitcoin.PubKey> _channels = [];

    public async Task ConnectToPeerAsync(PeerAddress peerAddress)
    {

        // Connect to the peer
        var tcpClient = new TcpClient();
        try
        {
            var cancellationToken = new CancellationTokenSource(_nodeOptions.NetworkTimeout).Token;
            await tcpClient.ConnectAsync(peerAddress.Host, peerAddress.Port, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new Exception($"Timeout connecting to peer {peerAddress.Host}:{peerAddress.Port}");
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to connect to peer {peerAddress.Host}:{peerAddress.Port}", e);
        }

        // Create and Initialize the transport service
        var transportService = _transportServiceFactory.CreateTransportService(true, _nodeOptions.KeyPair.PrivateKey.ToBytes(), peerAddress.PubKey.ToBytes(), tcpClient);
        await transportService.InitializeAsync(_nodeOptions.NetworkTimeout);

        var peer = new Peer(_nodeOptions, _messageServiceFactory.CreateMessageService(transportService), _pingPongServiceFactory.CreatePingPongService(_nodeOptions.NetworkTimeout), peerAddress, false);
        peer.DisconnectEvent += (sender, e) =>
        {
            _peers.Remove(peerAddress.PubKey);
        };

        _peers.Add(peerAddress.PubKey, peer);
    }

    public async Task AcceptPeerAsync(TcpClient tcpClient)
    {
        // Get peer data
        var remoteEndPoint = (IPEndPoint)(tcpClient.Client.RemoteEndPoint ?? throw new Exception("Failed to get remote endpoint"));
        var ipAddress = remoteEndPoint.Address.ToString();
        var port = remoteEndPoint.Port;

        // Create and Initialize the transport service
        var transportService = _transportServiceFactory.CreateTransportService(false, _nodeOptions.KeyPair.PrivateKey.ToBytes(), _nodeOptions.KeyPair.PublicKey.ToBytes(), tcpClient);
        await transportService.InitializeAsync(_nodeOptions.NetworkTimeout);
        if (transportService.RemoteStaticPublicKey == null)
        {
            throw new ErrorException("Failed to get remote static public key");
        }
        var peerAddress = new PeerAddress(transportService.RemoteStaticPublicKey, ipAddress, port);

        // Create the peer
        var peer = new Peer(_nodeOptions, _messageServiceFactory.CreateMessageService(transportService), _pingPongServiceFactory.CreatePingPongService(_nodeOptions.NetworkTimeout), peerAddress, true);

        peer.DisconnectEvent += (sender, e) =>
        {
            _peers.Remove(peerAddress.PubKey);
        };

        _peers.Add(peerAddress.PubKey, peer);
    }
}