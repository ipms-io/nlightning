using System.Net;
using System.Net.Sockets;
using Docker.DotNet;
using Docker.DotNet.Models;
using LNUnit.Setup;

namespace NLightning.Integration.Tests.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class PostgresFixture : IDisposable
{
    private const string ContainerName = "postgres";
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();
    private string? _containerId;
    private string? _ip;

    public PostgresFixture()
    {
        StartPostgres().GetAwaiter().GetResult();
    }

    public string? DbConnectionString { get; private set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        // Remove containers
        RemoveContainer(ContainerName).GetAwaiter().GetResult();

        _client.Dispose();
    }

    public async Task StartPostgres()
    {
        await _client.PullImageAndWaitForCompleted("postgres", "16.2-alpine");
        await RemoveContainer(ContainerName);
        var nodeContainer = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "postgres:16.2-alpine",
            HostConfig = new HostConfig
            {
                NetworkMode = "bridge"
            },
            Name = $"{ContainerName}",
            Hostname = $"{ContainerName}",
            Env =
            [
                "POSTGRES_PASSWORD=superuser",
                "POSTGRES_USER=superuser",
                "POSTGRES_DB=nlightning"
            ]
        }) ?? throw new NullReferenceException("Failed to create postgres container");
        _containerId = nodeContainer.ID;
        _ = await _client.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());

        //Build connection string
        var ipAddressReady = false;
        while (!ipAddressReady)
        {
            var listContainers = await _client.Containers.ListContainersAsync(new ContainersListParameters());

            var db = listContainers.FirstOrDefault(x => x.ID == nodeContainer.ID);
            if (db != null)
            {
                _ip = db.NetworkSettings.Networks.First().Value.IPAddress;
                DbConnectionString = $"Host={_ip};Database=nlightning;Username=superuser;Password=superuser";
                ipAddressReady = true;
            }
            else
            {
                await Task.Delay(100);
            }
        }

        //wait for TCP socket to open
        var tcpConnectable = false;
        while (!tcpConnectable)
        {
            try
            {
                TcpClient c = new()
                {
                    ReceiveTimeout = 1,
                    SendTimeout = 1
                };
                if (_ip != null)
                    await c.ConnectAsync(new IPEndPoint(IPAddress.Parse(_ip), 5432));

                if (c.Connected)
                {
                    tcpConnectable = true;
                }
            }
            catch (Exception)
            {
                await Task.Delay(50);
            }
        }
    }

    private async Task RemoveContainer(string name)
    {
        try
        {
            await _client.Containers.RemoveContainerAsync(name,
                                                          new ContainerRemoveParameters
                                                          { Force = true, RemoveVolumes = true });
        }
        catch
        {
            // ignored
        }
    }

    public async Task<bool> IsRunning()
    {
        try
        {
            var inspect = await _client.Containers.InspectContainerAsync(ContainerName);
            return inspect.State.Running;
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