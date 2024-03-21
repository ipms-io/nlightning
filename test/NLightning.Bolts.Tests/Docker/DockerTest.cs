using System.Collections;
using System.Diagnostics;
using System.Net.Mime;
using Docker.DotNet;
using Docker.DotNet.Models;
using NBitcoin;
using NBitcoin.RPC;
using NLightning.Bolts.Tests.Utils;
using ServiceStack.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace NLightning.Bolts.Tests.Docker;
 
public class DockerFixture : IDisposable
{
    public readonly DockerClient Client = new DockerClientConfiguration().CreateClient();
    
    public DockerFixture()
    {
        //We use this image for tests
        Client.PullImageAndWaitForCompleted("redis", "5.0").Wait();
    }
    public void Dispose()
    {
        Client.Dispose();
    }
}

public class DockerTest: IClassFixture<DockerFixture> 
{
    private readonly Random _random = new();

    private bool _sampleImagePulled;
    private readonly DockerClient _client;
 
    public DockerTest(DockerFixture f,ITestOutputHelper output)
    {
        _client = f.Client;
        Console.SetOut(new TestOutputWriter(output));
    }
    
    private void RemoveContainer(string name)
    {
        try
        {
            _client.Containers.RemoveContainerAsync(name,
                new ContainerRemoveParameters {Force = true, RemoveVolumes = true});
        }
        catch
        {
            // ignored
        }
    }

    [Fact]
    public async Task SetupBitcoinNode()
    {
        var image = "polarlightning/bitcoind";
        var tag = "26.0";
        await _client.PullImageAndWaitForCompleted(image, tag);
        var nodeContainer = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = $"{image}:{tag}",
            HostConfig = new HostConfig
            {
                NetworkMode = $"bridge"
            },
            Name = "btc_0",
            Hostname = "btc_0",
            Cmd = new List<string>
            {
                @"bitcoind",
                @"-server=1",
                $"-regtest=1",
                @"-rpcauth=bitcoin:c8c8b9740a470454255b7a38d4f38a52$e8530d1c739a3bb0ec6e9513290def11651afbfd2b979f38c16ec2cf76cf348a",
                @"-debug=1",
                @"-zmqpubrawblock=tcp://0.0.0.0:28334",
                @"-zmqpubrawtx=tcp://0.0.0.0:28335",
                @"-zmqpubhashblock=tcp://0.0.0.0:28336",
                @"-txindex=1",
                @"-dnsseed=0",
                @"-upnp=0",
                @"-rpcbind=0.0.0.0",
                @"-rpcallowip=0.0.0.0/0",
                @"-rpcport=18443",
                @"-rest",
                @"-rpcworkqueue=1024",
                @"-listen=1",
                @"-listenonion=0",
                @"-fallbackfee=0.0002"
            }
        });
        var DockerContainerId = nodeContainer.ID;
        var success =
            await _client.Containers.StartContainerAsync(nodeContainer.ID, new ContainerStartParameters());
        await Task.Delay(500);
        //Setup wallet and basic funds
        var listContainers = await _client.Containers.ListContainersAsync(new ContainersListParameters());
        var bitcoinNode = listContainers.First(x => x.ID == nodeContainer.ID);
        var bitcoinRpcClient = new RPCClient("bitcoin:bitcoin",
            bitcoinNode.NetworkSettings.Networks.First().Value.IPAddress, Bitcoin.Instance.Regtest);
        bitcoinRpcClient.HttpClient.Timeout = TimeSpan.FromMilliseconds(10000); //10s
        await bitcoinRpcClient.CreateWalletAsync("default", new CreateWalletOptions {LoadOnStartup = true});
        var utxos = await bitcoinRpcClient.GenerateAsync(200);

        var balance = await bitcoinRpcClient.GetBalanceAsync();
        $"Running on Container Id: {DockerContainerId}".Print(); 
        $"Bitcoin Balance: {balance.Satoshi / 1e8} BTC".Print();
        RemoveContainer("btc_0");
    }

    [Fact]
   // [Category("Docker")]
    public async Task ListContainer()
    {
        IList<ContainerListResponse> containers = await _client.Containers.ListContainersAsync(
            new ContainersListParameters
            {
                Limit = 10
            });
        // Assert.That(containers.Any());
    }


    [Fact]
//    [Category("Docker")]
    public async Task MakeContainer()
    {
        await PullRedis5(); 
        var x = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "redis:5.0",
            HostConfig = new HostConfig
            {
                DNS = new List<string>
                {
                    "8.8.8.8"
                }
            }
        });
        await _client.Containers.RemoveContainerAsync(x.ID,
            new ContainerRemoveParameters {RemoveVolumes = true, Force = true});
    }

    [Fact]
 //   [Category("Docker")]
    public async Task Docker_PullImage()
    {
        await PullRedis5();
    }

    private async Task PullRedis5()
    {
        if (_sampleImagePulled)
            return;
        await _client.PullImageAndWaitForCompleted("redis", "5.0");
        _sampleImagePulled = true;
    }


    [Fact]
//    [Category("Docker")]
    public async Task CreateDestroyNetwork()
    {
        var randomString = GetRandomHexString();
        var networksCreateResponse = await _client.Networks.CreateNetworkAsync(new NetworksCreateParameters
        {
            Name = $"unit_test_{randomString}",
            Driver = "bridge"
            //CheckDuplicate = true
        });
        await _client.Networks.DeleteNetworkAsync(networksCreateResponse.ID);
    }

    private string GetRandomHexString(int size = 8)
    {
        var b = new byte[size];
        _random.NextBytes(b);
        return Convert.ToHexString(b);
    }


   //  [Fact]
   // // [Category("Docker")]
   //  public async Task BuildDockerImage()
   //  {
   //      await _client.CreateDockerImageFromPath("./../../../../Docker/", new List<string> {"polar_lnd_0_16_1_test"});
   //  }


    [Fact]
   // [Category("Docker")]
    public async Task DetectGitlabRunner()
    {
        var result = await _client.GetGitlabRunnerNetworkId();
        Debug.Print(result);
    }

    // public string GetIPAddress()
    // {
    //     string IPAddress = "";
    //     IPHostEntry Host = default(IPHostEntry);
    //     string Hostname = null;
    //     Hostname = System.Environment.MachineName;
    //     Host = Dns.GetHostEntry(Hostname);
    //     foreach (IPAddress IP in Host.AddressList)
    //     {
    //         if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    //         {
    //             IPAddress = Convert.ToString(IP);
    //         }
    //     }
    //
    //     return IPAddress;
    // }

    [Fact]
    // [Category("Docker")]
    public async Task ComposeAndDestroyCluster()
    {
        await PullRedis5();
        var randomString = GetRandomHexString();
        var networksCreateResponse = await _client.Networks.CreateNetworkAsync(new NetworksCreateParameters
        {
            Name = $"unit_test_{randomString}",
            Driver = "bridge"
            // CheckDuplicate = true
        });
        Assert.Empty(networksCreateResponse.Warning);

        var alice = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "redis:5.0",
            HostConfig = new HostConfig
            {
                NetworkMode = $"unit_test_{randomString}"
            }
        });
        Assert.Empty(alice.Warnings);

        var bob = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "redis:5.0",
            HostConfig = new HostConfig
            {
                NetworkMode = $"unit_test_{randomString}"
            }
        });
        Assert.Empty(bob.Warnings);

        await _client.Containers.RemoveContainerAsync(alice.ID,
            new ContainerRemoveParameters {RemoveVolumes = true, Force = true});
        await _client.Containers.RemoveContainerAsync(bob.ID,
            new ContainerRemoveParameters {RemoveVolumes = true, Force = true});
        await _client.Networks.DeleteNetworkAsync(networksCreateResponse.ID);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}