// ReSharper disable PropertyCanBeMadeInitOnly.Global

using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Infrastructure.Persistence.Entities.Channel;

/// <summary>
/// Represents a set of cryptographic keys associated with a Lightning Network channel.
/// </summary>
public class ChannelKeySetEntity
{
    /// <summary>
    /// The unique channel identifier this key set belongs to.
    /// </summary>
    /// <remarks>Part of the composite primary key.</remarks>
    public required ChannelId ChannelId { get; set; }

    /// <summary>
    /// Indicates whether this key set belongs to the local node or remote node.
    /// </summary>
    /// <remarks>Part of the composite primary key.</remarks>
    public bool IsLocal { get; set; }

    /// <summary>
    /// The funding public key used to create the multisig funding output.
    /// </summary>
    public required byte[] FundingPubKey { get; set; }

    /// <summary>
    /// The base point for generating revocation keys.
    /// </summary>
    public required byte[] RevocationBasepoint { get; set; }

    /// <summary>
    /// The base point for generating payment keys.
    /// </summary>
    public required byte[] PaymentBasepoint { get; set; }

    /// <summary>
    /// The base point for generating delayed payment keys.
    /// </summary>
    public required byte[] DelayedPaymentBasepoint { get; set; }

    /// <summary>
    /// The base point for generating HTLC keys.
    /// </summary>
    public required byte[] HtlcBasepoint { get; set; }

    /// <summary>
    /// The current per-commitment index used in a channel's key set.
    /// </summary>
    /// <remarks>
    /// This index tracks the state of the commitment transaction sequence
    /// in the channel. It is incremented with each new commitment point.
    /// </remarks>
    public required ulong CurrentPerCommitmentIndex { get; set; }

    /// <summary>
    /// The current per-commitment point being used for the active commitment transaction.
    /// </summary>
    public required byte[] CurrentPerCommitmentPoint { get; set; }

    /// <summary>
    /// For remote key sets: stores their last revealed per-commitment secret
    /// This is needed to create penalty transactions if they broadcast old commitments
    /// For local key sets: this should be null (we don't store our own secrets)
    /// </summary>

    public byte[]? LastRevealedPerCommitmentSecret { get; set; }

    /// <summary>
    /// The index representing the key derivation progress for this channel key set.
    /// </summary>
    /// <remarks>Used to track the current state of key generation in the channel.</remarks>
    public required uint KeyIndex { get; set; }

    /// <summary>
    /// Default constructor for EF Core.
    /// </summary>
    internal ChannelKeySetEntity()
    {
    }
}