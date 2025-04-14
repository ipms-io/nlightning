using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using Lnrpc;
using NBitcoin;
using NLightning.Common.Interfaces;
using ServiceStack;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.Docker;

using BOLT1.Fixtures;
using Bolts.BOLT1.Factories;
using Bolts.BOLT1.Managers;
using Common.Constants;
using Common.Managers;
using Common.Node;
using TestCollections;
using Tests.Fixtures;
using Utils;

// ReSharper disable AccessToDisposedClosure
#pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
[Collection(SecureKeyAndConfigAndRegtestCollection.NAME)]
public class AbcNetworkTests
{
    private readonly LightningRegtestNetworkFixture _lightningRegtestNetworkFixture;
    private readonly Mock<ILogger> _mockLogger = new();

    public AbcNetworkTests(LightningRegtestNetworkFixture fixture, ITestOutputHelper output)
    {
        _lightningRegtestNetworkFixture = fixture;
        Console.SetOut(new TestOutputWriter(output));
    }

    [Fact]
    public async Task NLightning_BOLT8_Test_Connect_Alice()
    {
        // Arrange
        var hex = BitConverter.ToString(SecureKeyManager.GetPrivateKey().PubKey.ToBytes()).Replace("-", "");

        var alice = _lightningRegtestNetworkFixture.Builder?.LNDNodePool?.ReadyNodes.First(x => x.LocalAlias == "alice");
        Assert.NotNull(alice);

        ConfigManager.NodeOptions = new NodeOptions
        {
            ChainHashes = [ChainConstants.REGTEST],
            EnableDataLossProtect = true,
            EnableStaticRemoteKey = true,
            EnablePaymentSecret = true
        };
        IPeerManager peerManager = new PeerManager(new TransportServiceFactory(), new PingPongServiceFactory(),
                                                   new MessageServiceFactory(), _mockLogger.Object);

        var aliceHost = new IPEndPoint((await Dns.GetHostAddressesAsync(alice.Host
                                                                             .SplitOnFirst("//")[1]
                                                                             .SplitOnFirst(":")[0])).First(), 9735);

        // Act
        await peerManager.ConnectToPeerAsync(new Common.Types.PeerAddress(new PubKey(alice.LocalNodePubKeyBytes),
                                                                          aliceHost.Address.ToString(), aliceHost.Port));
        var alicePeers = alice.LightningClient.ListPeers(new ListPeersRequest());

        // Assert
        Assert.NotNull(alicePeers.Peers.FirstOrDefault(x => x.PubKey.Equals(hex, StringComparison.CurrentCultureIgnoreCase)));

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public async Task NLightning_BOLT8_Test_Bob_Connect()
    {
        // Arrange
        var availablePort = await PortPool.GetAvailablePortAsync();
        var listener = new TcpListener(IPAddress.Any, availablePort);
        listener.Start();

        try
        {
            // Get ip from host
            var hostAddress = Environment.GetEnvironmentVariable("HOST_ADDRESS") ?? "host.docker.internal";

            var hex = BitConverter.ToString(SecureKeyManager.GetPrivateKey().PubKey.ToBytes()).Replace("-", "");

            var bob = _lightningRegtestNetworkFixture.Builder?.LNDNodePool?.ReadyNodes.First(x =>
                x.LocalAlias == "bob");
            Assert.NotNull(bob);

            ConfigManager.NodeOptions = new NodeOptions
            {
                ChainHashes = [ChainConstants.REGTEST],
                EnableDataLossProtect = true,
                EnableStaticRemoteKey = true,
                EnablePaymentSecret = true
            };
            IPeerManager peerManager = new PeerManager(new TransportServiceFactory(), new PingPongServiceFactory(),
                new MessageServiceFactory(), _mockLogger.Object);

            var acceptTask = Task.Run(async () =>
            {
                {
                    var tcpClient = await listener.AcceptTcpClientAsync();

                    await peerManager.AcceptPeerAsync(tcpClient);
                }
            });
            await Task.Delay(1000);

            // Act
            await bob.LightningClient.ConnectPeerAsync(new ConnectPeerRequest
            {
                Addr = new LightningAddress
                {
                    Host = $"{hostAddress}:{availablePort}",
                    Pubkey = hex
                }
            });
            var alicePeers = bob.LightningClient.ListPeers(new ListPeersRequest());
            await acceptTask;

            // Assert
            Assert.NotNull(alicePeers.Peers.FirstOrDefault(x =>
                x.PubKey.Equals(hex, StringComparison.CurrentCultureIgnoreCase)));
        }
        finally
        {
            listener.Dispose();
            PortPool.ReleasePort(availablePort);

            ConfigManagerUtil.ResetConfigManager();
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
// ReSharper restore AccessToDisposedClosure