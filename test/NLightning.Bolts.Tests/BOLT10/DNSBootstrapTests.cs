
using NLightning.Bolts.BOLT10.Services;
using NLightning.Bolts.Tests.Utils;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.BOLT10;

public class DnsBootstrapTests 
    {
        
        public DnsBootstrapTests(ITestOutputHelper output)
        {
            Console.SetOut(new TestOutputWriter(output));
        }

        [Fact]
        public void Get_Bootstrap_Nodes_From_DNS_Seeds_IPv4()
        {
            var results =DnsSeedClient.FindNodes(10, new List<string>() { "nodes.lightning.directory", "lseed.bitcoinstats.com" },
                false);
            Assert.True(results.Count > 0, "No seeds returned.");
            foreach (var r in results)
            {
                $"{Convert.ToHexString(r.Pubkey).ToLower()}@{r.Endpoint.ToString().Replace(" ",string.Empty)}".Print();
            }
        }
        
        [Fact]
        public void Get_Bootstrap_Nodes_From_DNS_Seeds_IPv6()
        {
            var results =DnsSeedClient.FindNodes(10, new List<string>() { "nodes.lightning.directory", "lseed.bitcoinstats.com" },
                true);
            Assert.True(results.Count > 0, "No seeds returned.");
            foreach (var r in results)
            {
                $"{Convert.ToHexString(r.Pubkey).ToLower()}@{r.Endpoint.ToString().Replace(" ",string.Empty)}".Print();
            }
        }
        
     
    }