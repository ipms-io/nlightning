namespace NLightning.Bolts.Tests.Utils;

using Common.Enums;
using Common.Managers;
using Common.Types;

public static class ConfigManagerUtil
{
    public static void ResetConfigManager()
    {
        ConfigManager.Instance.Network = Network.MAIN_NET;
        ConfigManager.Instance.DustLimitAmount = LightningMoney.FromUnit(546, LightningMoneyUnit.SATOSHI);
        ConfigManager.Instance.AnchorAmount = LightningMoney.FromUnit(330, LightningMoneyUnit.SATOSHI);
        ConfigManager.Instance.MaxHtlcValueInFlightAmount = LightningMoney.Zero;
        ConfigManager.Instance.HtlcMinimumAmount = LightningMoney.Zero;
        ConfigManager.Instance.ToSelfDelay = 0;
        ConfigManager.Instance.MaxAcceptedHtlcs = 0;
        ConfigManager.Instance.Locktime = 0;
        ConfigManager.Instance.MinimumDepth = 0;
        ConfigManager.Instance.IsOptionAnchorOutput = true;
        ConfigManager.Instance.DefaultCltvExpiry = 0;
        ConfigManager.Instance.MustTrimHtlcOutputs = false;
        ConfigManager.Instance.FeeEstimationUrl = "https://mempool.space/api/v1/fees/recommended";
        ConfigManager.Instance.FeeEstimationMethod = "GET";
        ConfigManager.Instance.FeeEstimationBody = string.Empty;
        ConfigManager.Instance.FeeEstimationContentType = "application/json";
        ConfigManager.Instance.FeeEstimationPreferredFeeRate = "fastestFee";
        ConfigManager.Instance.FeeRateMultiplier = "1000";
        ConfigManager.Instance.FeeEstimationCacheFile = "fee_estimation_cache.json";
        ConfigManager.Instance.FeeEstimationCacheExpiration = "5m"; // 5 minutes
    }
}