namespace NLightning.Common.Options;

public class FeeEstimationOptions
{
    public string Url { get; set; } = "https://mempool.space/api/v1/fees/recommended";
    public string Method { get; set; } = "GET";
    public string Body { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
    public string PreferredFeeRate { get; set; } = "fastestFee";
    public string CacheFile { get; set; } = "fee_estimation_cache.bin";
    public string CacheExpiration { get; set; } = "5m"; // 5 minutes
    public string RateMultiplier { get; set; } = "1000";
}