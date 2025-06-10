namespace NLightning.Domain.Channels.ValueObjects;

using Crypto.ValueObjects;

public record struct ChannelBasepoints
{
    public CompactPubKey FundingPubKey { get; init; }
    public CompactPubKey RevocationBasepoint { get; init; }
    public CompactPubKey PaymentBasepoint { get; init; }
    public CompactPubKey DelayedPaymentBasepoint { get; init; }
    public CompactPubKey HtlcBasepoint { get; init; }

    public ChannelBasepoints(CompactPubKey fundingPubKey, CompactPubKey revocationBasepoint,
                             CompactPubKey paymentBasepoint, CompactPubKey delayedPaymentBasepoint,
                             CompactPubKey htlcBasepoint)
    {
        FundingPubKey = fundingPubKey;
        RevocationBasepoint = revocationBasepoint;
        PaymentBasepoint = paymentBasepoint;
        DelayedPaymentBasepoint = delayedPaymentBasepoint;
        HtlcBasepoint = htlcBasepoint;
    }
}