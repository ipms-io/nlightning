using Docker.DotNet;
using Docker.DotNet.Models;
using LNUnit.Setup;

namespace NLightning.Integration.Tests.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class LightningRegtestNetworkFixture : IDisposable
{
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();

    public LightningRegtestNetworkFixture()
    {
        SetupNetwork().GetAwaiter().GetResult();
    }

    public LNUnitBuilder? Builder { get; private set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        // Remove containers
        RemoveContainer("miner").GetAwaiter().GetResult();
        RemoveContainer("alice").GetAwaiter().GetResult();
        RemoveContainer("bob").GetAwaiter().GetResult();
        RemoveContainer("carol").GetAwaiter().GetResult();

        Builder?.Destroy();
        _client.Dispose();
    }

    public async Task SetupNetwork()
    {
        await RemoveContainer("miner");
        await RemoveContainer("alice");
        await RemoveContainer("bob");
        await RemoveContainer("carol");

        await _client.CreateDockerImageFromPath("../../../../Docker/custom_lnd", ["custom_lnd", "custom_lnd:latest"]);
        Builder = new LNUnitBuilder();

        Builder.AddBitcoinCoreNode();

        Builder.AddPolarLNDNode("alice",
        [
            new()
            {
                ChannelSize = 10_000_000, //10MSat
                RemoteName = "bob"
            }
        ], imageName: "custom_lnd", tagName: "latest", pullImage: false);

        Builder.AddPolarLNDNode("bob",
        [
            new()
            {
                ChannelSize = 10_000_000, //10MSat
                RemotePushOnStart = 1_000_000, // 1MSat
                RemoteName = "alice"
            }
        ], imageName: "custom_lnd", tagName: "latest", pullImage: false);

        Builder.AddPolarLNDNode("carol",
        [
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
        ], imageName: "custom_lnd", tagName: "latest", pullImage: false);

        await Builder.Build();
    }

    private async Task RemoveContainer(string name)
    {
        try
        {
            await _client.Containers.RemoveContainerAsync(
                name, new ContainerRemoveParameters { Force = true, RemoveVolumes = true });
        }
        catch
        {
            // ignored
        }
    }
}