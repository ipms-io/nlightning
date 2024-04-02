using System.Net.Sockets;

namespace NLightning.Bolts.BOLT1.Services;

using System.Net;
using BOLT1.Primitives;
using BOLT8.Services;
using Common;

public sealed class PeerService(NodeOptions nodeOptions)
{
    private readonly NodeOptions _nodeOptions = nodeOptions;
    private readonly Dictionary<NBitcoin.PubKey, Peer> _peers = [];
    private readonly Dictionary<ChannelId, NBitcoin.PubKey> _channels = [];

    public async Task ConnectToPeerAsync(PeerAddress peerAddress)
    {
        // Create a cancelation token that expires in 30 seconds
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;

        // Connect to the peer
        var tcpClient = new TcpClient();
        try
        {
            await tcpClient.ConnectAsync(peerAddress.Host, peerAddress.Port, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new Exception($"Failed to connect to peer {peerAddress.Host}:{peerAddress.Port} within 30 seconds");
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to connect to peer {peerAddress.Host}:{peerAddress.Port}", e);
        }

        // Create and Initialize the transport service
        var transportService = new TransportService(true, _nodeOptions.KeyPair.PrivateKey.ToBytes(), peerAddress.PubKey.ToBytes(), tcpClient);
        await transportService.InitializeAsync();

        // Create the message service
        var messageService = new MessageService(transportService);

        var peer = new Peer(_nodeOptions, messageService, peerAddress, false);
        peer.Disconect += (sender, e) =>
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
        var transportService = new TransportService(false, _nodeOptions.KeyPair.PrivateKey.ToBytes(), _nodeOptions.KeyPair.PublicKey.ToBytes(), tcpClient);
        await transportService.InitializeAsync();

        // Create the message service
        var messageService = new MessageService(transportService);

        var peerAddress = new PeerAddress(transportService.RemoteStaticPublicKey, ipAddress, port);

        // Create the peer
        var peer = new Peer(_nodeOptions, messageService, peerAddress, true);

        peer.Disconect += (sender, e) =>
        {
            _peers.Remove(peerAddress.PubKey);
        };

        _peers.Add(peerAddress.PubKey, peer);
    }
}