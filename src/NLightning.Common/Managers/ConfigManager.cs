namespace NLightning.Common.Managers;

using Types;

public class ConfigManager
{
    private static readonly Lazy<ConfigManager> s_instance = new(() => new ConfigManager());

    // Private constructor to prevent external instantiation
    private ConfigManager() { }

    // Accessor for the singleton instance
    public static ConfigManager Instance => s_instance.Value;

    /// <summary>
    /// Network in which this node will be running
    /// </summary>
    public Network Network { get; set; }

    /// <summary>
    /// DustLimitSatoshis is the threshold below which outputs should not be generated for this node's commitment or
    /// HTLC transactions (i.e. HTLCs below this amount plus HTLC transaction fees are not enforceable on-chain).
    /// This reflects the reality that tiny outputs are not considered standard transactions and will not propagate
    /// through the Bitcoin network.
    /// </summary>
    public ulong DustLimitSatoshis { get; set; }

    /// <summary>
    /// MaxHtlcValueInFlightMsat is a cap on total value of outstanding HTLCs offered by the remote node, which allows
    /// the local node to limit its exposure to HTLCs
    /// </summary>
    public ulong MaxHtlcValueInFlightMsat { get; set; }

    /// <summary>
    /// HtlcMinimumMsat indicates the smallest value HTLC this node will accept.
    /// </summary>
    public ulong HtlcMinimumMsat { get; set; }

    /// <summary>
    /// ToSelfDelay is the number of blocks that the other node's to-self outputs must be delayed, using
    /// OP_CHECKSEQUENCEVERIFY delays; this is how long it will have to wait in case of breakdown before redeeming its
    /// own funds.
    /// </summary>
    public ushort ToSelfDelay { get; set; }

    /// <summary>
    /// MaxAcceptedHtlcs limits the number of outstanding HTLCs the remote node can offer.
    /// </summary>
    public ushort MaxAcceptedHtlcs { get; set; }

    /// <summary>
    /// Locktime is the default locktime for the funding transaction.
    /// </summary>
    public uint Locktime { get; set; }

    /// <summary>
    /// minimum_depth is the number of blocks we consider reasonable to avoid double-spending of the funding transaction.
    /// In case channel_type includes option_zeroconf this MUST be 0
    /// </summary>
    public uint MinimumDepth { get; set; }
}