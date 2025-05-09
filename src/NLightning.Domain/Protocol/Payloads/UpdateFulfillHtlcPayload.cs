using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

/// <summary>
/// Represents the payload for the update_fulfill_htlc message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UpdateFulfillHtlcPayload class.
/// </remarks>
public class UpdateFulfillHtlcPayload(ChannelId channelId, ulong id, ReadOnlyMemory<byte> paymentPreimage) : IMessagePayload
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

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Id));
        await stream.WriteAsync(PaymentPreimage);
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<UpdateFulfillHtlcPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[sizeof(ulong)];
            await stream.ReadExactlyAsync(bytes);
            var id = EndianBitConverter.ToUInt64BigEndian(bytes);

            var paymentPreimage = new byte[32];
            await stream.ReadExactlyAsync(paymentPreimage);

            return new UpdateFulfillHtlcPayload(channelId, id, paymentPreimage);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing UpdateFulfillHtlcPayload", e);
        }
    }
}