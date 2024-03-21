using Docker.DotNet;
using Docker.DotNet.Models;
using LNUnit.Setup;
using Microsoft.EntityFrameworkCore;

namespace NLightning.Bolts.Tests.Docker.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class PostgresFixture : IDisposable
{
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();

    public PostgresFixture()
    {
        StartPostgres().Wait();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        // Remove containers
        RemoveContainer("postgres").Wait();

        _client.Dispose();
    }

    public async Task StartPostgres()
    {
        await _client.PullImageAndWaitForCompleted("postgres", "16.2-alpine");
        await RemoveContainer("postgres");
        var nodeContainer = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = $"postgres:16.2-alpine",
            HostConfig = new HostConfig
            {
                NetworkMode = $"bridge"
            },
            Name = "postgres",
            Hostname = "postgres",
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
            var inspect = _client.Containers.InspectContainerAsync("postgres");
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

public class QuickContext : DbContext
{
    public QuickContext(DbContextOptions<QuickContext> options)
        : base(options)
    {
    }

    public DbSet<X> Xs { get; set; }

    public class X
    {
        public int Id { get; set; }
    }
}