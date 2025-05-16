namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using ValueObjects;

/// <summary>
/// Represents the payload for the stfu message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the StfuPayload class.
/// </remarks>
public class StfuPayload(ChannelId channelId, bool initiator) : IMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// 1 if we're sending this message, 0 if we're responding
    /// </summary>
    public bool Initiator { get; } = initiator;
}