using Docker.DotNet;
using Docker.DotNet.Models;
using LNUnit.Setup;

namespace NLightning.Bolts.Tests.Docker.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global

public class PostgresFixture : IDisposable
{
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();
    private const string ContainerName = "postgres";

    public PostgresFixture()
    {

        StartPostgres().Wait();

    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        // Remove containers
        RemoveContainer(ContainerName).Wait();

        _client.Dispose();
    }

    public async Task StartPostgres()
    {
        await _client.PullImageAndWaitForCompleted("postgres", "16.2-alpine");
        await RemoveContainer(ContainerName);
        var nodeContainer = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = $"postgres:16.2-alpine",
            HostConfig = new HostConfig
            {
                NetworkMode = $"bridge"
            },
            Name = $"{ContainerName}",
            Hostname = $"{ContainerName}",
            Env = [
                "POSTGRES_PASSWORD=superuser",
                "POSTGRES_USER=superuser",
                "POSTGRES_DB=nlightning"
            ]
        });
    }

    private async Task RemoveContainer(string name)
    {
        try
        {
            await _client.Containers.RemoveContainerAsync(name, new ContainerRemoveParameters { Force = true, RemoveVolumes = true });
        }
        catch
        {
            // ignored
        }
    }

    public bool IsRunning()
    {
        try
        {
            var inspect = _client.Containers.InspectContainerAsync(ContainerName);
            inspect.Wait();
            return inspect.Result.State.Running;
        }
        catch
        {
            // ignored
        }
        return false;
    }
}

[CollectionDefinition("postgres")]
public class PostgresFixtureCollection : ICollectionFixture<PostgresFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}