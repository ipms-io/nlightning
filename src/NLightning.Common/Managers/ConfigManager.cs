using NLightning.Common.Enums;

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
    public Network Network { get; set; } = Network.MAIN_NET;

    /// <summary>
    /// DustLimitAmount is the threshold below which outputs should not be generated for this node's commitment or
    /// HTLC transactions (i.e. HTLCs below this amount plus HTLC transaction fees are not enforceable on-chain).
    /// This reflects the reality that tiny outputs are not considered standard transactions and will not propagate
    /// through the Bitcoin network.
    /// </summary>
    public LightningMoney DustLimitAmount { get; set; } = LightningMoney.FromUnit(546, LightningMoneyUnit.SATOSHI);

    public LightningMoney AnchorAmount { get; set; } = LightningMoney.FromUnit(330, LightningMoneyUnit.SATOSHI);

    /// <summary>
    /// MaxHtlcValueInFlightAmount is a cap on total value of outstanding HTLCs offered by the remote node, which allows
    /// the local node to limit its exposure to HTLCs
    /// </summary>
    public LightningMoney MaxHtlcValueInFlightAmount { get; set; } = LightningMoney.Zero;

    /// <summary>
    /// HtlcMinimumAmount indicates the smallest value HTLC this node will accept.
    /// </summary>
    public LightningMoney HtlcMinimumAmount { get; set; } = LightningMoney.Zero;

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

    /// <summary>
    /// is_option_anchor_outputs is a boolean indicating whether the node supports option_anchor_outputs.
    /// </summary>
    public bool IsOptionAnchorOutput { get; set; } = true;

    /// <summary>
    /// is_option_simple_close is a boolean indicating whether the node supports option_simple_close.
    /// </summary>
    public bool IsOptionSimpleClose => true;

    /// <summary>
    /// VTrue if outputs should be trimmed according to dust limits.
    /// </summary>
    public bool MustTrimHtlcOutputs { get; set; }

    /// <summary>
    /// default_cltv_expiry is the default CLTV expiry for HTLC outputs.
    /// </summary>
    public ulong DefaultCltvExpiry { get; set; }

    #region Fee Estimation
    public string FeeEstimationUrl { get; set; } = "https://mempool.space/api/v1/fees/recommended";
    public string FeeEstimationMethod { get; set; } = "GET";
    public string FeeEstimationBody { get; set; } = string.Empty;
    public string FeeEstimationContentType { get; set; } = "application/json";
    public string FeeEstimationPreferredFeeRate { get; set; } = "fastestFee";
    public string FeeRateMultiplier { get; set; } = "1000";
    public string FeeEstimationCacheFile { get; set; } = "fee_estimation_cache.json";
    public string FeeEstimationCacheExpiration { get; set; } = "5m"; // 5 minutes
    #endregion
}