namespace NLightning.Domain.Protocol.Payloads;

using Channels.ValueObjects;
using Interfaces;

/// <summary>
/// Represents the payload for the tx_ack_rbf message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TxAckRbfPayload class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
public class TxAckRbfPayload(ChannelId channelId) : IChannelMessagePayload
{
    /// <summary>
    /// Gets the channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;
}