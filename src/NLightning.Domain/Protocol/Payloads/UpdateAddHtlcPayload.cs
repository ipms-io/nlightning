namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Money;
using ValueObjects;

/// <summary>
/// Represents the payload for the update_add_htlc message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TxAckRbfPayload class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
public class UpdateAddHtlcPayload(LightningMoney amount, ChannelId channelId, uint cltvExpiry, ulong id,
                                  ReadOnlyMemory<byte> paymentHash, ReadOnlyMemory<byte>? onionRoutingPacket = null)
    : IMessagePayload
{
    /// <summary>
    /// Gets the channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// Offer Id
    /// </summary>
    /// <remarks>
    ///  This should be 0 for the first offer for the channel and must be incremented by 1 for each successive offer
    /// </remarks>
    public ulong Id { get; } = id;

    /// <summary>
    /// AmountSats offered for this Htlc
    /// </summary>
    public LightningMoney Amount { get; } = amount;

    /// <summary>
    /// The payment hash
    /// </summary>
    public ReadOnlyMemory<byte> PaymentHash { get; } = paymentHash;

    /// <summary>
    /// The Cltv Expiration
    /// </summary>
    public uint CltvExpiry { get; } = cltvExpiry;

    /// <summary>
    /// 
    /// </summary>
    public ReadOnlyMemory<byte>? OnionRoutingPacket { get; } = onionRoutingPacket;
}