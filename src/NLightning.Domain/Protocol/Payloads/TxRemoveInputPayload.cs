using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Messages;
using ValueObjects;

/// <summary>
/// Represents a tx_remove_input payload.
/// </summary>
/// <remarks>
/// The tx_remove_input payload is used to remove an input from the transaction.
/// </remarks>
/// <param name="channelId">The channel id.</param>
/// <param name="serialId">The serial id.</param>
/// <seealso cref="TxRemoveInputMessage"/>
/// <seealso cref="Channels.ValueObjects.ChannelId"/>
public class TxRemoveInputPayload(ChannelId channelId, ulong serialId) : IChannelMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The serial id.
    /// </summary>
    public ulong SerialId { get; } = serialId;
}