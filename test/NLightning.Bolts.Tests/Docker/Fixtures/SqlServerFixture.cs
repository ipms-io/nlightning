using System.Net;
using System.Net.Sockets;
using Docker.DotNet;
using Docker.DotNet.Models;
using LNUnit.Setup;

namespace NLightning.Bolts.Tests.Docker.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class SqlServerFixture : IDisposable
{
    private const string ContainerName = "sqlserver";
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();
    private string _containerId;
    private string _ip;

    public SqlServerFixture()
    {
        StartSqlServer().Wait();
    }

    public string DbConnectionString { get; private set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        // Remove containers
        RemoveContainer(ContainerName).Wait();

        _client.Dispose();
    }

    public async Task StartSqlServer()
    {

        await _client.PullImageAndWaitForCompleted("mcr.microsoft.com/mssql/server", "2022-latest");
        await RemoveContainer(ContainerName);
        var nodeContainer = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "mcr.microsoft.com/mssql/server:2022-latest",
            HostConfig = new HostConfig
            {
                NetworkMode = "bridge"
            },
            Name = $"{ContainerName}",
            Hostname = $"{ContainerName}",
            Env =
            [
                "MSSQL_SA_PASSWORD=Superuser1234*",
                "ACCEPT_EULA=Y"
            ]
        });

        Assert.NotNull(nodeContainer);
        _containerId = nodeContainer.ID;
        var started = await _client.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
        while (!IsRunning)
        {
            await Task.Delay(100);
        }

        //Build connection string
        var ipAddressReady = false;
        while (!ipAddressReady)
        {
            var listContainers = await _client.Containers.ListContainersAsync(new ContainersListParameters());

            var db = listContainers.FirstOrDefault(x => x.ID == nodeContainer.ID);
            if (db != null)
            {
                _ip = db.NetworkSettings.Networks.First().Value.IPAddress;
                DbConnectionString = $"Server={_ip};Database=tempdb;User Id=sa;Password=Superuser1234*;Trust Server Certificate=True;";
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
                await c.ConnectAsync(new IPEndPoint(IPAddress.Parse(_ip), 1433));
                if (c.Connected)
                {
                    tcpConnectable = true;
                }
                c.Dispose();
            }
            catch (Exception e)
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

    public bool IsRunning
    {
        get
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
}

[CollectionDefinition("sqlserver")]
public class SqlServerFixtureCollection : ICollectionFixture<SqlServerFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}