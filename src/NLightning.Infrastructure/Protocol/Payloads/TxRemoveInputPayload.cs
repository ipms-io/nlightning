using NLightning.Domain.Protocol.Messages;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

/// <summary>
/// Represents a tx_remove_input payload.
/// </summary>
/// <remarks>
/// The tx_remove_input payload is used to remove an input from the transaction.
/// </remarks>
/// <param name="channelId">The channel id.</param>
/// <param name="serialId">The serial id.</param>
/// <seealso cref="TxRemoveInputMessage"/>
/// <seealso cref="ValueObjects.ChannelId"/>
public class TxRemoveInputPayload(ChannelId channelId, ulong serialId) : IMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The serial id.
    /// </summary>
    public ulong SerialId { get; } = serialId;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(SerialId));
    }

    /// <summary>
    /// Deserialize a TxRemoveInputPayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxRemoveInputPayload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<TxRemoveInputPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            var serialId = EndianBitConverter.ToUInt64BigEndian(bytes);

            return new TxRemoveInputPayload(channelId, serialId);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing TxRemoveInputPayload", e);
        }
    }
}