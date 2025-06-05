using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Messages;

/// <summary>
/// Represents a tx_complete payload.
/// </summary>
/// <remarks>
/// The tx_complete payload is used to indicate that the transaction is complete.
/// </remarks>
/// <param name="channelId">The channel id.</param>
/// <seealso cref="TxCompleteMessage"/>
/// <seealso cref="Channels.ValueObjects.ChannelId"/>
public class TxCompletePayload(ChannelId channelId) : IChannelMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;
}