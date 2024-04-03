using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using Lnrpc;
using NBitcoin;
using ServiceStack;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.Docker;

using BOLT1.Fixtures;
using Bolts.BOLT1.Factories;
using Bolts.BOLT1.Primitives;
using Bolts.BOLT1.Services;
using Bolts.BOLT8.Dhs;
using Common.Constants;
using Fixtures;
using Utils;

#pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
[Collection("regtest")]
public class AbcNetworkTests
{
    private readonly LightningRegtestNetworkFixture _lightningRegtestNetworkFixture;

    public AbcNetworkTests(LightningRegtestNetworkFixture fixture, ITestOutputHelper output)
    {
        _lightningRegtestNetworkFixture = fixture;
        Console.SetOut(new TestOutputWriter(output));
    }

    [Fact]
    public async Task NLightning_BOLT8_Test_Connect_Alice()
    {
        // Arrange
        var localKeys = new Secp256k1().GenerateKeyPair();
        var hex = BitConverter.ToString(localKeys.PublicKey.ToBytes()).Replace("-", "");

        var alice = _lightningRegtestNetworkFixture.Builder?.LNDNodePool?.ReadyNodes.First(x => x.LocalAlias == "alice");
        Assert.NotNull(alice);

        var nodeOptions = new NodeOptions
        {
            ChainHashes = [ChainConstants.Regtest],
            EnableDataLossProtect = true,
            EnableStaticRemoteKey = true,
            EnablePaymentSecret = true,
            KeyPair = localKeys
        };
        var peerService = new PeerService(nodeOptions, new TransportServiceFactory(), new PingPongServiceFactory(), new MessageServiceFactory());

        var aliceHost = new IPEndPoint((await Dns.GetHostAddressesAsync(alice.Host.SplitOnFirst("//")[1].SplitOnFirst(":")[0])).First(), 9735);

        // Act
        await peerService.ConnectToPeerAsync(new PeerAddress(new PubKey(alice.LocalNodePubKeyBytes), aliceHost.Address.ToString(), aliceHost.Port));
        var alicePeers = alice.LightningClient.ListPeers(new ListPeersRequest());

        // Assert
        Assert.NotNull(alicePeers.Peers.Where(x => x.PubKey.Equals(hex, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault());
    }

    [Fact]
    public async Task NLightning_BOLT8_Test_Alice_Connect()
    {
        // Arrange
        var availablePort = await PortPool.GetAvailablePortAsync();
        var listener = new TcpListener(IPAddress.Any, availablePort);
        listener.Start();

        try
        {
            // Get ip from host
            var hostAddress = Environment.GetEnvironmentVariable("HOST_ADDRESS") ?? "host.docker.internal";

            var localKeys = new Secp256k1().GenerateKeyPair();
            var hex = BitConverter.ToString(localKeys.PublicKey.ToBytes()).Replace("-", "");

            var alice = _lightningRegtestNetworkFixture.Builder?.LNDNodePool?.ReadyNodes.First(x => x.LocalAlias == "alice");
            Assert.NotNull(alice);

            var nodeOptions = new NodeOptions
            {
                ChainHashes = [ChainConstants.Regtest],
                EnableDataLossProtect = true,
                EnableStaticRemoteKey = true,
                EnablePaymentSecret = true,
                KeyPair = localKeys
            };
            var peerService = new PeerService(nodeOptions, new TransportServiceFactory(), new PingPongServiceFactory(), new MessageServiceFactory());

            _ = Task.Run(async () =>
            {
                var tcpClient = await listener.AcceptTcpClientAsync();

                await peerService.AcceptPeerAsync(tcpClient);
            });
            await Task.Delay(1000);

            // Act
            await alice.LightningClient.ConnectPeerAsync(new ConnectPeerRequest
            {
                Addr = new LightningAddress
                {
                    Host = $"{hostAddress}:{availablePort}",
                    Pubkey = hex
                }
            });
            var alicePeers = alice.LightningClient.ListPeers(new ListPeersRequest());

            // Assert
            Assert.NotNull(alicePeers.Peers.Where(x => x.PubKey.Equals(hex, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault());
        }
        finally
        {
            listener.Dispose();
            PortPool.ReleasePort(availablePort);
        }
    }

    [Fact]
    public async Task Verify_Alice_Bob_Carol_Setup()
    {
        var readyNodes = _lightningRegtestNetworkFixture.Builder!.LNDNodePool!.ReadyNodes.ToImmutableList();
        var nodeCount = readyNodes.Count;
        Assert.Equal(3, nodeCount);
        $"LND Nodes in Ready State: {nodeCount}".Print();
        foreach (var node in readyNodes)
        {
            var walletBalanceResponse = await node.LightningClient.WalletBalanceAsync(new WalletBalanceRequest());
            var channels = await node.LightningClient.ListChannelsAsync(new ListChannelsRequest());
            $"Node {node.LocalAlias} ({node.LocalNodePubKey})".Print();
            walletBalanceResponse.PrintDump();
            channels.PrintDump();
        }

        $"Bitcoin Node Balance: {(await _lightningRegtestNetworkFixture.Builder!.BitcoinRpcClient!.GetBalanceAsync()).Satoshi / 1e8}".Print();
    }
}
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture