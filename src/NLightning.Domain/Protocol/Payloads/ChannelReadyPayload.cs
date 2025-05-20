using NBitcoin;

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
public class ChannelReadyPayload(ChannelId channelId, PubKey secondPerCommitmentPoint) : IMessagePayload
{
    /// <summary>
    /// Gets the channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    public PubKey SecondPerCommitmentPoint { get; } = secondPerCommitmentPoint;
}