using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT1.Payloads;

using Bolts.Interfaces;
using Common.BitUtils;

/// <summary>
/// Represents a Pong payload.
/// </summary>
/// <remarks>
/// A Pong payload is used to respond to a Ping payload.
/// </remarks>
/// <param name="bytesLen">The number of bytes in the pong payload.</param>
/// <seealso cref="Messages.PongMessage"/>
public class PongPayload(ushort bytesLen) : IMessagePayload
{
    /// <summary>
    /// The length of the ignored bytes.
    /// </summary>
    public ushort BytesLength { get; private set; } = bytesLen;

    /// <summary>
    /// The ignored bytes.
    /// </summary>
    public byte[] Ignored { get; private set; } = new byte[bytesLen];

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(BytesLength));
        await stream.WriteAsync(Ignored);
    }

    /// <summary>
    /// Deserialize a PongPayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized PongPayload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<PongPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var buffer = new byte[2];
            await stream.ReadExactlyAsync(buffer);
            var bytesLength = EndianBitConverter.ToUInt16BigEndian(buffer);

            var ignored = new byte[bytesLength];
            await stream.ReadExactlyAsync(ignored);

            return new PongPayload(bytesLength)
            {
                Ignored = ignored
            };
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing PongPayload", e);
        }
    }
}