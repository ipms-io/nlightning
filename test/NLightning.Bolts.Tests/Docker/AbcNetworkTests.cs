using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Docker.DotNet;
using Docker.DotNet.Models;
using Lnrpc;
using LNUnit.Setup;
using NLightning.Bolts.BOLT8.Services;
using NLightning.Bolts.Tests.BOLT8.Utils;
using NLightning.Bolts.Tests.Utils;
using ServiceStack;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.Docker;

// ReSharper disable once ClassNeverInstantiated.Global
public class LightningRegtestNetworkFixture : IDisposable
{
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();

    public LightningRegtestNetworkFixture()
    {
        SetupNetwork().Wait();
    }

    public LNUnitBuilder? Builder { get; private set; }

    public void Dispose()
    {
        Builder?.Destroy();
        _client.Dispose();
    }

    private async Task SetupNetwork()
    {
        RemoveContainer("miner");
        RemoveContainer("alice");
        RemoveContainer("bob");
        RemoveContainer("carol");

        await _client.CreateDockerImageFromPath("../../../../Docker/custom_lnd",
            ["custom_lnd", "custom_lnd:latest"]);
        Builder = new LNUnitBuilder();

        Builder.AddBitcoinCoreNode();
        Builder.AddPolarLNDNode("alice", new List<LNUnitNetworkDefinition.Channel>
        {
            new()
            {
                ChannelSize = 10_000_000, //10MSat
                RemoteName = "bob"
            }
        }, imageName: "custom_lnd", tagName: "latest", pullImage: false);
        Builder.AddPolarLNDNode("bob", new List<LNUnitNetworkDefinition.Channel>
        {
            new()
            {
                ChannelSize = 10_000_000, //10MSat
                RemotePushOnStart = 1_000_000, // 1MSat
                RemoteName = "alice"
            }
        }, imageName: "custom_lnd", tagName: "latest", pullImage: false);
        Builder.AddPolarLNDNode("carol", new List<LNUnitNetworkDefinition.Channel>
        {
            new()
            {
                ChannelSize = 10_000_000, //10MSat
                RemotePushOnStart = 1_000_000, // 1MSat
                RemoteName = "alice"
            },
            new()
            {
                ChannelSize = 10_000_000, //10MSat
                RemotePushOnStart = 1_000_000, // 1MSat
                RemoteName = "bob"
            }
        }, imageName: "custom_lnd", tagName: "latest", pullImage: false);
        await Builder.Build();
    }

    private void RemoveContainer(string name)
    {
        try
        {
            _client.Containers.RemoveContainerAsync(name,
                new ContainerRemoveParameters { Force = true, RemoveVolumes = true }).Wait();
        }
        catch
        {
            // ignored
        }
    }
}

public class AbcNetworkTests : IClassFixture<LightningRegtestNetworkFixture>
{
    private readonly LightningRegtestNetworkFixture _lightningRegtestNetworkFixture;

    public AbcNetworkTests(LightningRegtestNetworkFixture f, ITestOutputHelper output)
    {
        _lightningRegtestNetworkFixture = f;
        Console.SetOut(new TestOutputWriter(output));
    }


    [Fact]
    public async Task NLightning_BOLT8_Test_Connect_Alice()
    {
        var alice = _lightningRegtestNetworkFixture.Builder?.LNDNodePool?.ReadyNodes.First(x => x.LocalAlias == "alice");
        Assert.NotNull(alice);
        var tcpClient1 = new TcpClient();
        await tcpClient1.ConnectAsync(
            new IPEndPoint((await Dns.GetHostAddressesAsync(alice.Host.SplitOnFirst("//")[1].SplitOnFirst(":")[0])).First(), 9735));

        var handshakeService = new HandshakeService(true,
            InitiatorValidKeysUtil.EphemeralPrivateKey,
            alice.LocalNodePubKeyBytes);

        var transportService = new TransportService(handshakeService, tcpClient1);
        transportService.MessageReceived += (sender, bytes) =>
        {
            Debug.Print(Convert.ToHexString(bytes));
        };
        await transportService.Initialize();
        await Task.Delay(1000);
    }

    [Fact]
    public async Task Verify_Alice_Bob_Carol_Setup()
    {
        var readyNodes = _lightningRegtestNetworkFixture.Builder!.LNDNodePool!.ReadyNodes.ToImmutableList();
        var nodeCount = readyNodes.Count();
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

        $"Bitcoin Node Balance: {(await _lightningRegtestNetworkFixture.Builder!.BitcoinRpcClient!.GetBalanceAsync()).Satoshi / 1e8}"
            .Print();
    }
}