namespace NLightning.Domain.Protocol.Payloads;

using Channels.ValueObjects;
using Crypto.ValueObjects;
using Interfaces;

/// <summary>
/// Represents the payload for the revoke_and_ack message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the RevokeAndAckPayload class.
/// </remarks>
public class RevokeAndAckPayload(
    ChannelId channelId,
    CompactPubKey nextPerCommitmentPoint,
    ReadOnlyMemory<byte> perCommitmentSecret) : IChannelMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// len is the per commitment secret
    /// </summary>
    public ReadOnlyMemory<byte> PerCommitmentSecret { get; } = perCommitmentSecret;

    /// <summary>
    /// The next per commitment point
    /// </summary>
    public CompactPubKey NextPerCommitmentPoint { get; } = nextPerCommitmentPoint;
}