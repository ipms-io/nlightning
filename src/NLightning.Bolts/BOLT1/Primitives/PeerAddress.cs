using System.Net;
using System.Text.RegularExpressions;
using NBitcoin;

namespace NLightning.Bolts.BOLT1.Primitives;

public sealed partial class PeerAddress
{
    public PubKey PubKey { get; }
    public IPAddress Host { get; }
    public int Port { get; }

    public PeerAddress(string address)
    {
        var parts = address.Split('@');
        PubKey = new PubKey(parts[0]);

        var hostPort = parts[1].Split(':');
        Host = IPAddress.Parse(hostPort[0]);
        Port = int.Parse(hostPort[1]);
    }

    public PeerAddress(PubKey pubKey, string address)
    {
        PubKey = pubKey;

        // Check if address contains starts with http
        if (address.StartsWith("http"))
        {
            // split on first // to get the address
            var host = address.Split("//")[1];
            Host = Dns.GetHostAddresses(host.Split(":")[0])[0];

            // Port my have an extra / at the end. use regex to keep only the number in the port
            Port = int.Parse(OnlyDigitsRegex().Match(host.Split(":")[1]).Value);
        }
        else
        {
            var parts = address.Split('@');
            var hostPort = parts[1].Split(':');
            Host = IPAddress.Parse(hostPort[0]);
        }
    }

    public PeerAddress(PubKey pubKey, string host, int port)
    {
        PubKey = pubKey;
        Host = IPAddress.Parse(host);
        Port = port;
    }

    public override string ToString()
    {
        return $"{PubKey}@{Host}:{Port}";
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex OnlyDigitsRegex();
}