using System.Text;

namespace NLightning.Common.Messages.Payloads;

using BitUtils;
using Exceptions;
using Interfaces;
using Types;

/// <summary>
/// Represents an error payload.
/// </summary>
/// <remarks>
/// An error payload is used to communicate an error to the other party.
/// </remarks>
/// <param name="data">The error data.</param>
/// <seealso cref="ErrorMessage"/>
public class ErrorPayload(byte[] data) : IMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    /// <remarks>
    /// The channel id is used to identify the channel the error is related to.
    /// </remarks>
    public ChannelId ChannelId { get; } = ChannelId.Zero;

    /// <summary>
    /// The error data.
    /// </summary>
    /// <remarks>
    /// The error data is used to communicate the error.
    /// </remarks>
    public byte[]? Data { get; } = data;

    public ErrorPayload(ChannelId channelId, byte[] data) : this(data)
    {
        ChannelId = channelId;
    }
    public ErrorPayload(ChannelId channelId, string message) : this(channelId, Encoding.UTF8.GetBytes(message))
    { }
    public ErrorPayload(string message) : this(Encoding.UTF8.GetBytes(message))
    { }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        if (Data == null)
        {
            await stream.WriteAsync(new byte[] { 0, 0 });
        }
        else
        {
            await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)Data.Length));
            await stream.WriteAsync(Data);
        }
    }

    /// <summary>
    /// Deserialize an ErrorPayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized ErrorPayload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<ErrorPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var buffer = new byte[2];
            await stream.ReadExactlyAsync(buffer);
            var dataLength = EndianBitConverter.ToUInt16BigEndian(buffer);

            var data = new byte[dataLength];
            await stream.ReadExactlyAsync(data);

            return new ErrorPayload(channelId, data);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing ErrorPayload", e);
        }
    }
}