using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using ValueObjects;

/// <summary>
/// Represents the payload for the channel_ready message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ChannelReadyPayload class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
public class ChannelReadyPayload(ChannelId channelId, CompactPubKey secondPerCommitmentPoint) : IChannelMessagePayload
{
    /// <summary>
    /// Gets the channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    public CompactPubKey SecondPerCommitmentPoint { get; } = secondPerCommitmentPoint;
}