using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT1.Payloads;

using Bolts.Interfaces;

public class PongPayload(ushort bytesLen) : IMessagePayload
{
    public ushort BytesLength { get; private set; } = bytesLen;
    public byte[] Ignored { get; private set; } = new byte[bytesLen];

    public async Task SerializeAsync(Stream stream)
    {
        await stream.WriteAsync(EndianBitConverter.GetBytesBE(BytesLength));
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
            var bytesLength = EndianBitConverter.ToUInt16BE(buffer);

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