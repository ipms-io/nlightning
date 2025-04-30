namespace NLightning.Domain.Protocol.Constants;

using Money;

public static class ChannelConstants
{
    public const int MAX_ACCEPTED_HTLCS = 483;

    public static readonly LightningMoney LARGE_CHANNEL_AMOUNT = LightningMoney.Satoshis(16_777_216);
    public static readonly LightningMoney MAX_FEE_PER_KW = LightningMoney.Satoshis(100_000);
    public static readonly LightningMoney MIN_FEE_PER_KW = LightningMoney.Satoshis(1_000);
    public static readonly LightningMoney MIN_DUST_LIMIT_AMOUNT = LightningMoney.Satoshis(354);
}