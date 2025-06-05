namespace NLightning.Domain.Channels.ValueObjects;

using Bitcoin.ValueObjects;
using Money;

public readonly record struct ChannelConfig
{
    public uint MinimumDepth { get; }
    public ushort ToSelfDelay { get; }
    public ushort MaxAcceptedHtlcs { get; }
    public LightningMoney LocalDustLimitAmount { get; }
    public LightningMoney RemoteDustLimitAmount { get; }
    public LightningMoney? ChannelReserveAmount { get; }
    public LightningMoney MaxHtlcAmountInFlight { get; }
    public LightningMoney FeeRateAmountPerKw { get; }
    public LightningMoney HtlcMinimumAmount { get; }
    public bool OptionAnchorOutputs { get; }
    public BitcoinScript? LocalUpfrontShutdownScript { get; }
    public BitcoinScript? RemoteShutdownScriptPubKey { get; }

    public ChannelConfig(LightningMoney? channelReserveAmount, LightningMoney localDustLimitAmount,
                         LightningMoney feeRateAmountPerKw, LightningMoney htlcMinimumAmount, ushort maxAcceptedHtlcs,
                         LightningMoney maxHtlcAmountInFlight, uint minimumDepth, bool optionAnchorOutputs,
                         LightningMoney remoteDustLimitAmount, ushort toSelfDelay,
                         BitcoinScript? localUpfrontShutdownScript = null,
                         BitcoinScript? remoteShutdownScriptPubKey = null)
    {
        MinimumDepth = minimumDepth;
        ToSelfDelay = toSelfDelay;
        MaxAcceptedHtlcs = maxAcceptedHtlcs;
        LocalDustLimitAmount = localDustLimitAmount;
        ChannelReserveAmount = channelReserveAmount;
        HtlcMinimumAmount = htlcMinimumAmount;
        MaxHtlcAmountInFlight = maxHtlcAmountInFlight;
        FeeRateAmountPerKw = feeRateAmountPerKw;
        OptionAnchorOutputs = optionAnchorOutputs;
        RemoteDustLimitAmount = remoteDustLimitAmount;
        LocalUpfrontShutdownScript = localUpfrontShutdownScript;
        RemoteShutdownScriptPubKey = remoteShutdownScriptPubKey;
    }
}