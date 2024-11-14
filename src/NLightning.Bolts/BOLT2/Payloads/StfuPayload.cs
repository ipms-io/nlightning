using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Payloads;

using Interfaces;

public class StfuPayload : IMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; set; }

    /// <summary>
    /// 1 if we're sending this message, 0 if we're responding
    /// </summary>
    public bool Initiator { get; set; }

    public StfuPayload(ChannelId channelId, bool initiator)
    {
        ChannelId = channelId;
        Initiator = initiator;
    }

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
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
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
            throw new SerializationException("Error deserializing StfuPayload", e);
        }
    }
}