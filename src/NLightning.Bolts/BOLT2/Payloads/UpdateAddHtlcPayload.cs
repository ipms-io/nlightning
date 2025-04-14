namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Common.Constants;
using Common.Interfaces;

/// <summary>
/// Represents the payload for the update_add_htlc message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TxAckRbfPayload class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
public class UpdateAddHtlcPayload(ChannelId channelId, ulong id, LightningMoney amount, ReadOnlyMemory<byte> paymentHash, uint cltvExpiry, ReadOnlyMemory<byte>? onionRoutingPacket = null) : IMessagePayload
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

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Id));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Amount.MilliSatoshi));
        await stream.WriteAsync(PaymentHash);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(CltvExpiry));
        if (OnionRoutingPacket is not null)
        {
            await stream.WriteAsync(OnionRoutingPacket.Value);
        }
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<UpdateAddHtlcPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var buffer = new byte[sizeof(ulong)];
            await stream.ReadExactlyAsync(buffer);
            var id = EndianBitConverter.ToUInt64BigEndian(buffer);

            await stream.ReadExactlyAsync(buffer);
            var amountMsat = EndianBitConverter.ToUInt64BigEndian(buffer);

            var paymentHash = new byte[CryptoConstants.SHA256_HASH_LEN];
            await stream.ReadExactlyAsync(paymentHash);

            buffer = new byte[sizeof(uint)];
            await stream.ReadExactlyAsync(buffer);
            var cltvExpiry = EndianBitConverter.ToUInt32BigEndian(buffer);

            byte[]? onionRoutingPacket = null;
            if (stream.Position + 1366 <= stream.Length)
            {
                onionRoutingPacket = new byte[1366];
                await stream.ReadExactlyAsync(onionRoutingPacket);
            }

            return new UpdateAddHtlcPayload(channelId, id, amountMsat, paymentHash, cltvExpiry, onionRoutingPacket);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing UpdateAddHtlcPayload", e);
        }
    }
}