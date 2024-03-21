using System.Collections.Immutable;
using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;
using Lnrpc;
using LNUnit.Setup;
using NBitcoin;
using NBitcoin.RPC;
using NLightning.Bolts.Tests.Utils;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.Docker;

public class LightningRegtestNetworkFixture : IDisposable
{
    public readonly DockerClient Client = new DockerClientConfiguration().CreateClient();
    public LNUnitBuilder Builder { get; private set; }
    public LightningRegtestNetworkFixture()
    { 
        
    }

    public async Task SetupNetwork()
    {
        RemoveContainer("miner");
        RemoveContainer("alice");
        RemoveContainer("bob");
        RemoveContainer("carol");

        await Client.CreateDockerImageFromPath("../../../../Docker/custom_lnd", new List<string> { "custom_lnd","custom_lnd:latest" });
        Builder = new LNUnitBuilder();

        Builder.AddBitcoinCoreNode();
        Builder.AddPolarLNDNode("alice", new List<LNUnitNetworkDefinition.Channel>
        {
            new()
            {
                ChannelSize = 10_000_000, //10MSat
                RemoteName = "bob"
            }
        }, imageName:"custom_lnd", tagName: "latest", pullImage:false);
        Builder.AddPolarLNDNode("bob", new List<LNUnitNetworkDefinition.Channel>
        {
            new()
            {
                ChannelSize = 10_000_000, //10MSat
                RemotePushOnStart = 1_000_000, // 1MSat
                RemoteName = "alice"
            }
        }, imageName:"custom_lnd", tagName: "latest", pullImage:false);
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
        }, imageName:"custom_lnd", tagName: "latest", pullImage:false);
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
    
    public void Dispose()
    {
        Builder.Destroy();
        Client.Dispose();
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
        _lightningRegtestNetworkFixture.SetupNetwork().Wait();
    }

    [Fact]
    public async Task VerifyABCNetworkSetup()
    {
        var nodeCount = _lightningRegtestNetworkFixture.Builder.LNDNodePool?.ReadyNodes.Count();
        Assert.Equal(nodeCount,3);
        $"LND Nodes in Ready State: {nodeCount}".Print();
        foreach (var node in _lightningRegtestNetworkFixture.Builder.LNDNodePool?.ReadyNodes.ToImmutableList())
        {
            var walletBalanceResponse = await node.LightningClient.WalletBalanceAsync(new WalletBalanceRequest() { });
            var channels = await node.LightningClient.ListChannelsAsync(new ListChannelsRequest() { });
            $"Node {node.LocalAlias} ({node.LocalNodePubKey})".Print();
            walletBalanceResponse.PrintDump();
            channels.PrintDump();
        }
        $"Bitcoin Node Balance: {_lightningRegtestNetworkFixture.Builder.BitcoinRpcClient?.GetBalance().Satoshi/1e8}".Print();
    } 
}