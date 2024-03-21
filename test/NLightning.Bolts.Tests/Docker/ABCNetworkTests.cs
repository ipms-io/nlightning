using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Docker.DotNet;
using Docker.DotNet.Models;
using Lnrpc;
using LNUnit.Setup;
using NLightning.Bolts.BOLT8.Services;
using NLightning.Bolts.BOLT8.States;
using NLightning.Bolts.Tests.BOLT8.Utils;
using NLightning.Bolts.Tests.Utils;
using ServiceStack;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.Docker;

public class LightningRegtestNetworkFixture : IDisposable
{
    public readonly DockerClient Client = new DockerClientConfiguration().CreateClient();

    public LightningRegtestNetworkFixture()
    {
        SetupNetwork().Wait();
    }

    public LNUnitBuilder Builder { get; private set; }

    public void Dispose()
    {
        Builder.Destroy();
        Client.Dispose();
    }

    public async Task SetupNetwork()
    {
        RemoveContainer("miner");
        RemoveContainer("alice");
        RemoveContainer("bob");
        RemoveContainer("carol");

        await Client.CreateDockerImageFromPath("../../../../Docker/custom_lnd",
            new List<string> { "custom_lnd", "custom_lnd:latest" });
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
            Client.Containers.RemoveContainerAsync(name,
                new ContainerRemoveParameters { Force = true, RemoveVolumes = true }).Wait();
        }
        catch
        {
            // ignored
        }
    }
}

public class ABCNetworkTests : IClassFixture<LightningRegtestNetworkFixture>
{
    private readonly DockerClient _client;
    private readonly LightningRegtestNetworkFixture _lightningRegtestNetworkFixture;

    public ABCNetworkTests(LightningRegtestNetworkFixture f, ITestOutputHelper output)
    {
        _lightningRegtestNetworkFixture = f;
        _client = f.Client;
        Console.SetOut(new TestOutputWriter(output));
    }


    [Fact]
    public async Task NLightning_BOLT8_Test_Connect_Alice()
    {
        var alice = _lightningRegtestNetworkFixture.Builder.LNDNodePool.ReadyNodes.First(x => x.LocalAlias == "alice");

        var initiator = new HandshakeState(true, InitiatorValidKeysUtil.LocalStaticPrivateKey,
            InitiatorValidKeysUtil.RemoteStaticPublicKey);

        var tcpClient1 = new TcpClient();
        tcpClient1.Connect(
            new IPEndPoint(Dns.GetHostAddresses(alice.Host.SplitOnFirst("//")[1].SplitOnFirst(":")[0]).First(), 9735));

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
    public async Task VerifyABCNetworkSetup()
    {
        var nodeCount = _lightningRegtestNetworkFixture.Builder.LNDNodePool?.ReadyNodes.Count();
        Assert.Equal(nodeCount, 3);
        $"LND Nodes in Ready State: {nodeCount}".Print();
        foreach (var node in _lightningRegtestNetworkFixture.Builder.LNDNodePool?.ReadyNodes.ToImmutableList())
        {
            var walletBalanceResponse = await node.LightningClient.WalletBalanceAsync(new WalletBalanceRequest());
            var channels = await node.LightningClient.ListChannelsAsync(new ListChannelsRequest());
            $"Node {node.LocalAlias} ({node.LocalNodePubKey})".Print();
            walletBalanceResponse.PrintDump();
            channels.PrintDump();
        }

        $"Bitcoin Node Balance: {_lightningRegtestNetworkFixture.Builder.BitcoinRpcClient?.GetBalance().Satoshi / 1e8}"
            .Print();
    }
}