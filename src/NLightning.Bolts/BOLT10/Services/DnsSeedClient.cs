using System.Net;
using DnsClient;
using DnsClient.Protocol;
using NBitcoin.DataEncoders;

namespace NLightning.Bolts.BOLT10.Services;

public static class DnsSeedClient
{
    public record NodeRecord(byte[] Pubkey, IPEndPoint Endpoint);

    /// <summary>
    /// Find Nodes from DNS seed domains
    /// </summary>
    /// <param name="nodeCount">Records to return</param>
    /// <param name="seeds">List of seed domains</param>
    /// <param name="ipV6">Return IPv6 endpoints</param>
    /// <param name="useTcp">Use TCP Only</param>
    /// <param name="nameServers">Provide your own nameservers to override system</param>
    /// <returns></returns>
    public static List<NodeRecord> FindNodes(int nodeCount, List<string> seeds, bool ipV6 = false, bool useTcp = false, params IPAddress[] nameServers)
    {
        var opts = nameServers.Length != 0 ? new LookupClientOptions(nameServers) : new LookupClientOptions();
        opts.UseTcpOnly = useTcp;
        var client = new LookupClient(opts);
        var list = new List<NodeRecord>();
        foreach (var dnsSeed in seeds)
        {
            if (list.Count < nodeCount)
            {
                try
                {
                    var srvResult = client.Query(dnsSeed, QueryType.SRV);
                    if (srvResult.HasError)
                        continue;

                    var srvShuffled = srvResult.Answers.OrderBy(_ => Guid.NewGuid()).ToList();

                    foreach (var srv in srvShuffled.SrvRecords())
                    {
                        if (list.Count >= nodeCount)
                        {
                            continue;
                        }

                        var result = client.Query(srv.Target, ipV6 ? QueryType.AAAA : QueryType.A);

                        if (result.Answers.Count <= 0)
                        {
                            continue;
                        }

                        var publicKey = GetPublicKey(srv);
                        var ip = GetIp(result.Answers[0]);

                        if (ip != "0.0.0.0" && ip != "[::0]")
                        {
                            list.Add(new NodeRecord(publicKey, new IPEndPoint(IPAddress.Parse(ip), srv.Port)));
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                break;
            }
        }

        return list;
    }

    private static string GetIp(DnsResourceRecord answer)
    {
        if (answer is ARecord record)
        {
            return record.Address.ToString();
        }

        return $"[{((AaaaRecord)answer).Address}]";
    }

    private static byte[] GetPublicKey(SrvRecord srv)
    {
        var bech32 = srv.Target.Value.Split('.').First();
        var bech32Encoder = Encoders.Bech32("ln");
        var bech32Data5Bits = bech32Encoder.DecodeDataRaw(bech32, out _);
        var bech32Data8Bits = ConvertBits(bech32Data5Bits, 5, 8, false);
        return bech32Data8Bits;
    }

    /*
     * The following method was copied from NBitcoin
     * https://github.com/MetacoSA/NBitcoin/blob/23beaaab48f2038dca24a6020e71cee0b14cd55f/NBitcoin/DataEncoders/Bech32Encoder.cs#L427
     */
    private static byte[] ConvertBits(IEnumerable<byte> data, int fromBits, int toBits, bool pad = true)
    {
        var num1 = 0;
        var num2 = 0;
        var num3 = (1 << toBits) - 1;
        var byteList = new List<byte>();
        foreach (var num4 in data)
        {
            if (num4 >> fromBits > 0)
                throw new FormatException("Invalid Bech32 string");
            num1 = num1 << fromBits | num4;
            num2 += fromBits;
            while (num2 >= toBits)
            {
                num2 -= toBits;
                byteList.Add((byte)(num1 >> num2 & num3));
            }
        }

        if (pad)
        {
            if (num2 > 0)
                byteList.Add((byte)(num1 << toBits - num2 & num3));
        }
        else if (num2 >= fromBits || (byte)(num1 << toBits - num2 & num3) != 0)
            throw new FormatException("Invalid Bech32 string");

        return [.. byteList];
    }
}