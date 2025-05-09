using NBitcoin;

namespace NLightning.Domain.Channels;

using ValueObjects;

public class ChannelKeyData
{
    public ChannelId ChannelId { get; set; }
    public uint KeyIndex { get; init; }
    public PubKey FundingPubKey { get; init; }
    public PubKey RevocationBasepoint { get; init; }
    public PubKey PaymentBasepoint { get; init; }
    public PubKey DelayedPaymentBasepoint { get; init; }
    public PubKey HtlcBasepoint { get; init; }
    
    public ChannelKeyData(ChannelId channelId, uint keyIndex, PubKey fundingPubKey, PubKey revocationBasepoint,
                          PubKey paymentBasepoint, PubKey delayedPaymentBasepoint, PubKey htlcBasepoint)
    {
        ChannelId = channelId;
        KeyIndex = keyIndex;
        FundingPubKey = fundingPubKey;
        RevocationBasepoint = revocationBasepoint;
        PaymentBasepoint = paymentBasepoint;
        DelayedPaymentBasepoint = delayedPaymentBasepoint;
        HtlcBasepoint = htlcBasepoint;
    }
}