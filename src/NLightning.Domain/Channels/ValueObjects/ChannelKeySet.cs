using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Domain.Channels.ValueObjects;

public readonly record struct ChannelKeySet
{
    public uint KeyIndex { get; }
    public CompactPubKey FundingCompactPubKey { get; }
    public CompactPubKey RevocationCompactBasepoint { get; }
    public CompactPubKey PaymentCompactBasepoint { get; }
    public CompactPubKey DelayedPaymentCompactBasepoint { get; }
    public CompactPubKey HtlcCompactBasepoint { get; }
    public CompactPubKey? GossipCompactPubkey { get; }
    public ulong CurrentPerCommitmentIndex { get; }
    public CompactPubKey CurrentPerCommitmentCompactPoint { get; }
    public ISecretStorageService SecretStorageService { get; }
    
    public ChannelKeySet(uint keyIndex, CompactPubKey fundingCompactPubKey, CompactPubKey revocationCompactBasepoint,
                         CompactPubKey paymentCompactBasepoint, CompactPubKey delayedPaymentCompactBasepoint,
                         CompactPubKey htlcCompactBasepoint, CompactPubKey? gossipCompactPubkey,
                         CompactPubKey currentPerCommitmentCompactPoint, ulong currentPerCommitmentIndex,
                         ISecretStorageService secretStorageService)
    {
        KeyIndex = keyIndex;
        FundingCompactPubKey = fundingCompactPubKey;
        RevocationCompactBasepoint = revocationCompactBasepoint;
        PaymentCompactBasepoint = paymentCompactBasepoint;
        DelayedPaymentCompactBasepoint = delayedPaymentCompactBasepoint;
        HtlcCompactBasepoint = htlcCompactBasepoint;
        GossipCompactPubkey = gossipCompactPubkey;
        CurrentPerCommitmentCompactPoint = currentPerCommitmentCompactPoint;
        SecretStorageService = secretStorageService;
        CurrentPerCommitmentIndex = currentPerCommitmentIndex;
    }
}