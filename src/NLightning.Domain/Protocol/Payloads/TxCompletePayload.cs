namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Messages;
using ValueObjects;

/// <summary>
/// Represents a tx_complete payload.
/// </summary>
/// <remarks>
/// The tx_complete payload is used to indicate that the transaction is complete.
/// </remarks>
/// <param name="channelId">The channel id.</param>
/// <seealso cref="TxCompleteMessage"/>
/// <seealso cref="ValueObjects.ChannelId"/>
public class TxCompletePayload(ChannelId channelId) : IMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;
}