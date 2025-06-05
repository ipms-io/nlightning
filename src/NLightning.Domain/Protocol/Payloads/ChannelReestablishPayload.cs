using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;

/// <summary>
/// Represents the payload for the channel_reestablish message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ChannelReestablishPayload class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
public class ChannelReestablishPayload(ChannelId channelId, CompactPubKey myCurrentPerCommitmentPoint,
                                       ulong nextCommitmentNumber, ulong nextRevocationNumber,
                                       ReadOnlyMemory<byte> yourLastPerCommitmentSecret) : IChannelMessagePayload
{
    /// <summary>
    /// Gets the channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The commitment transaction counter
    /// </summary>
    public ulong NextCommitmentNumber { get; } = nextCommitmentNumber;

    /// <summary>
    /// The commitment counter it expects for the next revoke and ack message 
    /// </summary>
    public ulong NextRevocationNumber { get; } = nextRevocationNumber;

    /// <summary>
    /// The last per commitment secret received
    /// </summary>
    public ReadOnlyMemory<byte> YourLastPerCommitmentSecret { get; } = yourLastPerCommitmentSecret;

    /// <summary>
    /// The current per commitment point
    /// </summary>
    public CompactPubKey MyCurrentPerCommitmentPoint { get; } = myCurrentPerCommitmentPoint;
}