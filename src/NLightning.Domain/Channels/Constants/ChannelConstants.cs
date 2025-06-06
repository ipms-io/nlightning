namespace NLightning.Domain.Channels.Constants;

using Money;

public static class ChannelConstants
{
    public const int MaxAcceptedHtlcs = 483;
    public const int ChannelIdLength = 32;
    public const int MaxUnconfirmedChannelAge = 2016; // 2 weeks at 6 blocks per hour

    public static readonly LightningMoney LargeChannelAmount = LightningMoney.Satoshis(16_777_216);
    public static readonly LightningMoney MaxFeePerKw = LightningMoney.Satoshis(100_000);
    public static readonly LightningMoney MinFeePerKw = LightningMoney.Satoshis(1_000);
    public static readonly LightningMoney MinDustLimitAmount = LightningMoney.Satoshis(354);
}