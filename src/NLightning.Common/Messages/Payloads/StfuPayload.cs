namespace NLightning.Common.Messages.Payloads;

using Exceptions;
using Interfaces;
using Types;

/// <summary>
/// Represents the payload for the stfu message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the StfuPayload class.
/// </remarks>
public class StfuPayload(ChannelId channelId, bool initiator) : IMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; set; } = channelId;

    /// <summary>
    /// 1 if we're sending this message, 0 if we're responding
    /// </summary>
    public bool Initiator { get; set; } = initiator;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(new ReadOnlyMemory<byte>([(byte)(Initiator ? 1 : 0)]));
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<StfuPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[1];
            await stream.ReadExactlyAsync(bytes);

            return new StfuPayload(channelId, bytes[0] == 1);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing StfuPayload", e);
        }
    }
}