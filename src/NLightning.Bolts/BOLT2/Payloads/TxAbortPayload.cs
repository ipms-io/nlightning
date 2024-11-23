namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Exceptions;
using Interfaces;

/// <summary>
/// Represents the payload for the tx_abort message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TxAbortPayload class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
/// <param name="data">The data.</param>
public class TxAbortPayload(ChannelId channelId, byte[] data) : IMessagePayload
{
    /// <summary>
    /// Gets the channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// Gets the data.
    /// </summary>
    public byte[] Data { get; } = data;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)Data.Length));
        await stream.WriteAsync(Data);
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<TxAbortPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[2];
            await stream.ReadExactlyAsync(bytes);
            var length = EndianBitConverter.ToUInt16BigEndian(bytes);

            var data = new byte[length];
            await stream.ReadExactlyAsync(data);

            return new TxAbortPayload(channelId, data);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing TxAbortPayload", e);
        }
    }
}