using System.Collections;
using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;
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

    public DockerTest(DockerFixture f)
    {
        _client = f.Client;
    }
    
    private void ResetContainer(string name)
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