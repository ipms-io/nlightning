using System.Runtime.Serialization;
using System.Text;

namespace NLightning.Bolts.BOLT1.Payloads;

using Bolts.Interfaces;

public class ErrorPayload(byte[] data) : IMessagePayload
{
    public ChannelId ChannelId { get; } = ChannelId.Zero;
    public byte[]? Data { get; } = data;

    public ErrorPayload(ChannelId channelId, byte[] data) : this(data)
    {
        ChannelId = channelId;
    }
    public ErrorPayload(ChannelId channelId, string message) : this(channelId, Encoding.UTF8.GetBytes(message))
    { }
    public ErrorPayload(string message) : this(Encoding.UTF8.GetBytes(message))
    { }

    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        if (Data == null)
        {
            await stream.WriteAsync(new byte[2] { 0, 0 });
        }
        else
        {
            await stream.WriteAsync(EndianBitConverter.GetBytesBE((ushort)Data.Length));
            await stream.WriteAsync(Data);
        }
    }

    /// <summary>
    /// Deserialize an ErrorPayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized ErrorPayload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<ErrorPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var buffer = new byte[2];
            await stream.ReadExactlyAsync(buffer);
            var dataLength = EndianBitConverter.ToUInt16BE(buffer);

            var data = new byte[dataLength];
            await stream.ReadExactlyAsync(data);

            return new ErrorPayload(channelId, data);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing ErrorPayload", e);
        }
    }
}