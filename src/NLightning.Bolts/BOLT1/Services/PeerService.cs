using System.Net.Sockets;
using NLightning.Bolts.BOLT1.Primitives;
using NLightning.Bolts.BOLT8.Services;
using NLightning.Common;

namespace NLightning.Bolts.BOLT1.Services;

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

        var peer = new Peer(_nodeOptions, messageService, peerAddress);
        peer.Disconect += (sender, e) =>
        {
            _peers.Remove(peerAddress.PubKey);
        };

        _peers.Add(peerAddress.PubKey, peer);
    }

    public async Task<Peer> AcceptPeerAsync(TcpClient tcpClient)
    {
        var transportService = new TransportService(false, _nodeOptions.KeyPair.PrivateKey.ToBytes(), _nodeOptions.KeyPair.PublicKey.ToBytes(), tcpClient);
        await transportService.InitializeAsync();

        var messageService = new MessageService(transportService);

        return new Peer(null, messageService, null);
    }
}