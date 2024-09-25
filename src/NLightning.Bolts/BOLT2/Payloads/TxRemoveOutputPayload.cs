using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Common.Types;
using Interfaces;

/// <summary>
/// Represents a tx_remove_output payload.
/// </summary>
/// <remarks>
/// The tx_remove_output payload is used to remove an output from the transaction.
/// </remarks>
/// <param name="channelId">The channel id.</param>
/// <param name="serialId">The serial id.</param>
/// <seealso cref="Messages.TxRemoveOutputMessage"/>
/// <seealso cref="Common.Types.ChannelId"/>
public class TxRemoveOutputPayload(ChannelId channelId, ulong serialId) : IMessagePayload
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
    /// Deserialize a TxRemoveOutputPayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxRemoveOutputPayload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<TxRemoveOutputPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            var serialId = EndianBitConverter.ToUInt64BigEndian(bytes);

            return new TxRemoveOutputPayload(channelId, serialId);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TxRemoveOutputPayload", e);
        }
    }
}