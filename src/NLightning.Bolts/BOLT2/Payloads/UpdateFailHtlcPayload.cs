namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Exceptions;
using Interfaces;

/// <summary>
/// Represents the payload for the update_fail_htlc message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UpdateFailHtlcPayload class.
/// </remarks>
public class UpdateFailHtlcPayload(ChannelId channelId, ulong id, ReadOnlyMemory<byte> reason) : IMessagePayload
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
    /// The length of the reason
    /// </summary>
    public ushort Len => (ushort)Reason.Length;

    /// <summary>
    /// The reason for failure
    /// </summary>
    public ReadOnlyMemory<byte> Reason { get; } = reason;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Id));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Len));
        await stream.WriteAsync(Reason);
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<UpdateFailHtlcPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var buffer = new byte[sizeof(ulong)];
            await stream.ReadExactlyAsync(buffer);
            var id = EndianBitConverter.ToUInt64BigEndian(buffer);

            buffer = new byte[sizeof(ushort)];
            await stream.ReadExactlyAsync(buffer);
            var len = EndianBitConverter.ToUInt16BigEndian(buffer);

            var reason = new byte[len];
            await stream.ReadExactlyAsync(reason);

            return new UpdateFailHtlcPayload(channelId, id, reason);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing UpdateFailHtlcPayload", e);
        }
    }
}