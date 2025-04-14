namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Common.Interfaces;

/// <summary>
/// Represents the payload for the update_fee message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UpdateFeePayload class.
/// </remarks>
public class UpdateFeePayload(ChannelId channelId, uint feeratePerKw) : IMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The fee rate per kw
    /// </summary>
    public uint FeeratePerKw { get; } = feeratePerKw;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FeeratePerKw));
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<UpdateFeePayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var buffer = new byte[sizeof(uint)];
            await stream.ReadExactlyAsync(buffer);
            var feeratePerKw = EndianBitConverter.ToUInt32BigEndian(buffer);

            return new UpdateFeePayload(channelId, feeratePerKw);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing UpdateFeePayload", e);
        }
    }
}