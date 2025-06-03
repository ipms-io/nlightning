using NLightning.Domain.Crypto;
using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Channels;

using ValueObjects;

public class ChannelKeyData
{
    public ChannelId ChannelId { get; set; }
    public uint KeyIndex { get; init; }
    public CompactPubKey FundingPubKey { get; init; }
    public CompactPubKey RevocationBasepoint { get; init; }
    public CompactPubKey PaymentBasepoint { get; init; }
    public CompactPubKey DelayedPaymentBasepoint { get; init; }
    public CompactPubKey HtlcBasepoint { get; init; }

    public ChannelKeyData(ChannelId channelId, uint keyIndex, CompactPubKey fundingPubKey, CompactPubKey revocationBasepoint,
        CompactPubKey paymentBasepoint, CompactPubKey delayedPaymentBasepoint, CompactPubKey htlcBasepoint)
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