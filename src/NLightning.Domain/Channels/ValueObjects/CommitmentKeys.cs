namespace NLightning.Domain.Channels.ValueObjects;

using NLightning.Domain.Crypto.ValueObjects;

public record struct CommitmentKeys
{
    /// <summary>
    /// The localpubkey from the commitment owner's perspective.
    /// Derived as: payment_basepoint + SHA256(per_commitment_point || payment_basepoint) * G
    /// Used for to_remote outputs (the other party can spend immediately).
    /// </summary>
    public CompactPubKey LocalPubKey { get; init; }

    /// <summary>
    /// The local_delayedpubkey from the commitment owner's perspective.
    /// Derived as: delayed_payment_basepoint + SHA256(per_commitment_point || delayed_payment_basepoint) * G
    /// Used for to_local outputs (the commitment owner can spend after a delay).
    /// </summary>
    public CompactPubKey LocalDelayedPubKey { get; init; }

    /// <summary>
    /// The revocationpubkey that allows the other party to revoke this commitment.
    /// Complex derivation involving both parties' keys to ensure neither can compute the private key alone.
    /// </summary>
    public CompactPubKey RevocationPubKey { get; init; }

    /// <summary>
    /// The local_htlcpubkey from the commitment owner's perspective.
    /// Derived as: htlc_basepoint + SHA256(per_commitment_point || htlc_basepoint) * G
    /// Used for HTLC outputs owned by the commitment owner.
    /// </summary>
    public CompactPubKey LocalHtlcPubKey { get; init; }

    /// <summary>
    /// The remote_htlcpubkey from the commitment owner's perspective.
    /// Derived as: remote_htlc_basepoint + SHA256(per_commitment_point || remote_htlc_basepoint) * G
    /// Used for HTLC outputs owned by the other party.
    /// </summary>
    public CompactPubKey RemoteHtlcPubKey { get; init; }

    /// <summary>
    /// The per-commitment point used to derive all the above keys.
    /// Generated as: per_commitment_secret * G
    /// </summary>
    public CompactPubKey PerCommitmentPoint { get; init; }

    /// <summary>
    /// The per-commitment secret used to generate the per-commitment point.
    /// Only available for our own commitments, not for remote commitments.
    /// </summary>
    public Secret? PerCommitmentSecret { get; init; }

    public CommitmentKeys(CompactPubKey localPubKey, CompactPubKey localDelayedPubKey,
                          CompactPubKey revocationPubKey, CompactPubKey localHtlcPubKey,
                          CompactPubKey remoteHtlcPubKey, CompactPubKey perCommitmentPoint,
                          Secret? perCommitmentSecret)
    {
        LocalPubKey = localPubKey;
        LocalDelayedPubKey = localDelayedPubKey;
        RevocationPubKey = revocationPubKey;
        LocalHtlcPubKey = localHtlcPubKey;
        RemoteHtlcPubKey = remoteHtlcPubKey;
        PerCommitmentPoint = perCommitmentPoint;
        PerCommitmentSecret = perCommitmentSecret;
    }
}