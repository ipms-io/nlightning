using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;

/// <summary>
/// Represents the payload for the update_fulfill_htlc message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UpdateFulfillHtlcPayload class.
/// </remarks>
public class UpdateFulfillHtlcPayload(ChannelId channelId, ulong id, ReadOnlyMemory<byte> paymentPreimage)
    : IChannelMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The htlc id
    /// </summary>
    public ulong Id { get; } = id;

    /// <summary>
    /// The payment_preimage for this htlc
    /// </summary>
    public ReadOnlyMemory<byte> PaymentPreimage { get; } = paymentPreimage;
}