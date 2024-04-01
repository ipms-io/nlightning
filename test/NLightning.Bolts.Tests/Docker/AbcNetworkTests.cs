using System.Collections.Immutable;
using Lnrpc;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.Docker;

using System.Net;
using Fixtures;
using NBitcoin;
using NLightning.Bolts.BOLT1.Primitives;
using NLightning.Bolts.BOLT1.Services;
using ServiceStack;
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
        var alice = _lightningRegtestNetworkFixture.Builder?.LNDNodePool?.ReadyNodes.First(x => x.LocalAlias == "alice");
        Assert.NotNull(alice);

        var nodeOptions = new NodeOptions
        {
            EnableDataLossProtect = true,
            EnableStaticRemoteKey = true,
            EnablePaymentSecret = true,
        };
        var peerService = new PeerService(nodeOptions);

        var aliceHost = new IPEndPoint((await Dns.GetHostAddressesAsync(alice.Host.SplitOnFirst("//")[1].SplitOnFirst(":")[0])).First(), 9735);
        await peerService.ConnectToPeerAsync(new PeerAddress(new PubKey(alice.LocalNodePubKeyBytes), aliceHost.Address.ToString(), aliceHost.Port));
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