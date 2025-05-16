namespace NLightning.Domain.Node.Options;

using Money;

public class NodeOptions
{
    /// <summary>
    /// The network to connect to. Can be "mainnet", "testnet", or "regtest"
    /// </summary>
    public string Network { get; set; } = "mainnet";

    /// <summary>
    /// True if NLTG should run in Daemon mode (background)
    /// </summary>
    public bool Daemon { get; set; }

    /// <summary>
    /// A list of dns seed servers to connect to
    /// </summary>
    public List<string> DnsSeedServers { get; set; } = ["nlseed.nlightn.ing"];

    /// <summary>
    /// Addresses/Interfaces to listen on for incoming connections
    /// </summary>
    /// <remarks>
    /// Addresses should be in the format "ip:port" or "hostname:port"
    /// </remarks>
    public List<string> ListenAddresses { get; set; } = ["127.0.0.1:9735"];

    /// <summary>
    /// List of Features the node offers/expects to/from peers
    /// </summary>
    /// <see cref="FeatureOptions"/>
    public FeatureOptions Features { get; set; } = new();

    /// <summary>
    /// Network timeout
    /// </summary>
    public TimeSpan NetworkTimeout { get; set; } = TimeSpan.FromSeconds(15);

    public LightningMoney AnchorAmount { get; set; } = LightningMoney.Satoshis(330);

    public bool MustTrimHtlcOutputs { get; set; }

    public LightningMoney DustLimitAmount { get; set; } = LightningMoney.Satoshis(546);

    public ulong DefaultCltvExpiry { get; set; }

    public bool HasAnchorOutputs => !AnchorAmount.IsZero;

    public ushort MaxAcceptedHtlcs { get; set; }
    public LightningMoney HtlcMinimumAmount { get; set; } = LightningMoney.Satoshis(1);
    public uint Locktime { get; set; }
    public ushort ToSelfDelay { get; set; }
    public LightningMoney MaxHtlcValueInFlight { get; set; } = LightningMoney.Satoshis(1_000_000);
    public uint MinimumDepth { get; set; } = 3;
}