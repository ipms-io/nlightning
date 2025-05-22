namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using ValueObjects;

/// <summary>
/// Represents the payload for the tx_abort message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TxAbortPayload class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
/// <param name="data">The data.</param>
public class TxAbortPayload(ChannelId channelId, byte[] data) : IChannelMessagePayload
{
    /// <summary>
    /// Gets the channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// Gets the data.
    /// </summary>
    public byte[] Data { get; } = data;
}