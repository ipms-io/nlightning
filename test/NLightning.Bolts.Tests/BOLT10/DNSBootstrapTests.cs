using System.Net;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.BOLT10;

using Bolts.BOLT10.Services;
using Utils;

/// <summary>
/// This tests do not run on Github CI for some reason DNS will not resolve
/// </summary>
public class DnsBootstrapTests
{

    public DnsBootstrapTests(ITestOutputHelper output)
    {
        Console.SetOut(new TestOutputWriter(output));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Get_Bootstrap_Nodes_From_DNS_Seeds_IPv4(bool tcpOnly)
    {
        if (!tcpOnly && Environment.GetEnvironmentVariable("CI") == "true")
        {
            return; // Skip this test on GitHub Actions if using UDP
        }

        var results = DnsSeedClient.FindNodes(10, new List<string>() { "nodes.lightning.directory", "lseed.bitcoinstats.com" },
            false, useTcp: tcpOnly, new IPAddress[] { IPAddress.Parse("8.8.8.8") });
        Assert.True(results.Count > 0, "No seeds returned.");
        foreach (var r in results)
        {
            $"{Convert.ToHexString(r.Pubkey).ToLower()}@{r.Endpoint.ToString().Replace(" ", string.Empty)}".Print();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Get_Bootstrap_Nodes_From_DNS_Seeds_IPv6(bool tcpOnly)
    {
        if (!tcpOnly && Environment.GetEnvironmentVariable("CI") == "true")
        {
            return; // Skip this test on GitHub Actions if using UDP
        }

        var results = DnsSeedClient.FindNodes(10, new List<string>() { "nodes.lightning.directory", "lseed.bitcoinstats.com" },
            true, useTcp: tcpOnly, new IPAddress[] { IPAddress.Parse("8.8.8.8") });
        //Don't asserts so few doesn't always return any.
        //Assert.True(results.Count > 0, "No seeds returned.");
        foreach (var r in results)
        {
            $"{Convert.ToHexString(r.Pubkey).ToLower()}@{r.Endpoint.ToString().Replace(" ", string.Empty)}".Print();
        }
    }
}