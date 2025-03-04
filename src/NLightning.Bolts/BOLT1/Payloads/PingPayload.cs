namespace NLightning.Bolts.BOLT1.Payloads;

using Bolts.Interfaces;
using Common.BitUtils;
using Exceptions;

/// <summary>
/// The ping payload.
/// </summary>
/// <remarks>
/// The ping payload is used to check if the other party is still alive.
/// </remarks>
/// <seealso cref="Messages.PingMessage"/>
public class PingPayload : IMessagePayload
{
    /// <summary>
    /// The maximum length of the ignored bytes.
    /// </summary>
    private const ushort MAX_LENGTH = 65531;

    /// <summary>
    /// The number of bytes to send in the pong message.
    /// </summary>
    public ushort NumPongBytes { get; internal init; }

    /// <summary>
    /// The number of bytes to ignore.
    /// </summary>
    public ushort BytesLength { get; internal init; }

    /// <summary>
    /// The ignored bytes.
    /// </summary>
    public byte[] Ignored { get; internal init; }

    public PingPayload()
    {
        // Get number of bytes at random between byte.MaxValue and ushort.MaxValue
        NumPongBytes = (ushort)new Random().Next(byte.MaxValue, MAX_LENGTH);
        BytesLength = (ushort)new Random().Next(byte.MaxValue, MAX_LENGTH);
        Ignored = new byte[BytesLength];
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(NumPongBytes));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(BytesLength));
        await stream.WriteAsync(Ignored);
    }

    /// <summary>
    /// Deserialize a PingPayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized PingPayload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<PingPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var buffer = new byte[2];
            await stream.ReadExactlyAsync(buffer);
            var numPongBytes = EndianBitConverter.ToUInt16BigEndian(buffer);

            await stream.ReadExactlyAsync(buffer);
            var bytesLength = EndianBitConverter.ToUInt16BigEndian(buffer);

            var ignored = new byte[bytesLength];
            await stream.ReadExactlyAsync(ignored);

            return new PingPayload
            {
                NumPongBytes = numPongBytes,
                BytesLength = bytesLength,
                Ignored = ignored
            };
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing PingPayload", e);
        }
    }
}