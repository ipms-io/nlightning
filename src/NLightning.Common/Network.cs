namespace NLightning.Common;

public class Network(string name)
{
    public static readonly Network MainNet = new("mainnet");
    public static readonly Network TestNet = new("testnet");
    public static readonly Network RegTest = new("regtest");
    public static readonly Network SigNet = new("signet");

    public string Name { get; } = name;

    public override string ToString() => Name;
}