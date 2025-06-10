namespace NLightning.Domain.Channels.Models;

using Crypto.Constants;
using Crypto.ValueObjects;

public class ChannelKeySetModel
{
    public uint KeyIndex { get; }
    public CompactPubKey FundingCompactPubKey { get; }
    public CompactPubKey RevocationCompactBasepoint { get; }
    public CompactPubKey PaymentCompactBasepoint { get; }
    public CompactPubKey DelayedPaymentCompactBasepoint { get; }

    public CompactPubKey HtlcCompactBasepoint { get; }
    public CompactPubKey CurrentPerCommitmentCompactPoint { get; private set; }
    public ulong CurrentPerCommitmentIndex { get; private set; }

    /// <summary>
    /// For remote key sets: stores their last revealed per-commitment secret
    /// This is needed to create penalty transactions if they broadcast old commitments
    /// For local key sets: this should be null (we don't store our own secrets)
    /// </summary>
    public byte[]? LastRevealedPerCommitmentSecret { get; private set; }

    public ChannelKeySetModel(uint keyIndex, CompactPubKey fundingCompactPubKey,
                              CompactPubKey revocationCompactBasepoint, CompactPubKey paymentCompactBasepoint,
                              CompactPubKey delayedPaymentCompactBasepoint, CompactPubKey htlcCompactBasepoint,
                              CompactPubKey currentPerCommitmentCompactPoint,
                              ulong currentPerCommitmentIndex = CryptoConstants.FirstPerCommitmentIndex,
                              byte[]? lastRevealedPerCommitmentSecret = null)
    {
        KeyIndex = keyIndex;
        FundingCompactPubKey = fundingCompactPubKey;
        RevocationCompactBasepoint = revocationCompactBasepoint;
        PaymentCompactBasepoint = paymentCompactBasepoint;
        DelayedPaymentCompactBasepoint = delayedPaymentCompactBasepoint;
        HtlcCompactBasepoint = htlcCompactBasepoint;
        CurrentPerCommitmentCompactPoint = currentPerCommitmentCompactPoint;
        CurrentPerCommitmentIndex = currentPerCommitmentIndex;
        LastRevealedPerCommitmentSecret = lastRevealedPerCommitmentSecret;
    }

    public void UpdatePerCommitmentPoint(CompactPubKey newPoint)
    {
        CurrentPerCommitmentCompactPoint = newPoint;
        CurrentPerCommitmentIndex--;
    }

    /// <summary>
    /// Store a revealed per-commitment secret from the counterparty
    /// This is called when they send a revoke_and_ack message
    /// </summary>
    public void RevealPerCommitmentSecret(byte[] secret)
    {
        LastRevealedPerCommitmentSecret = secret;
    }

    /// <summary>
    /// Create a ChannelKeySet for the remote party (we don't generate their keys)
    /// </summary>
    public static ChannelKeySetModel CreateForRemote(CompactPubKey fundingPubKey, CompactPubKey revocationBasepoint,
                                                     CompactPubKey paymentBasepoint,
                                                     CompactPubKey delayedPaymentBasepoint, CompactPubKey htlcBasepoint,
                                                     CompactPubKey firstPerCommitmentPoint)
    {
        return new ChannelKeySetModel(0, fundingPubKey, revocationBasepoint, paymentBasepoint, delayedPaymentBasepoint,
                                      htlcBasepoint, firstPerCommitmentPoint);
    }
}