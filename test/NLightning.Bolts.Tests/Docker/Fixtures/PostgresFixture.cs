using System.Net;
using System.Net.Sockets;
using Docker.DotNet;
using Docker.DotNet.Models;
using LNUnit.Setup;

namespace NLightning.Bolts.Tests.Docker.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class PostgresFixture : IDisposable
{
    private const string CONTAINER_NAME = "postgres";
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();
    private string? _containerId;
    private string? _ip;

    public PostgresFixture()
    {
        StartPostgres().Wait();
    }

    public string? DbConnectionString { get; private set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        // Remove containers
        RemoveContainer(CONTAINER_NAME).Wait();

        _client.Dispose();
    }

    public async Task StartPostgres()
    {
        await _client.PullImageAndWaitForCompleted("postgres", "16.2-alpine");
        await RemoveContainer(CONTAINER_NAME);
        var nodeContainer = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "postgres:16.2-alpine",
            HostConfig = new HostConfig
            {
                NetworkMode = "bridge"
            },
            Name = $"{CONTAINER_NAME}",
            Hostname = $"{CONTAINER_NAME}",
            Env =
            [
                "POSTGRES_PASSWORD=superuser",
                "POSTGRES_USER=superuser",
                "POSTGRES_DB=nlightning"
            ]
        });
        Assert.NotNull(nodeContainer);
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
                new ContainerRemoveParameters { Force = true, RemoveVolumes = true });
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
            var inspect = await _client.Containers.InspectContainerAsync(CONTAINER_NAME);
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