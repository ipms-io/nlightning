using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Payloads;

using Interfaces;

/// <summary>
/// Represents the payload for the tx_ack_rbf message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TxAckRbfPayload"/> class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
public class TxAckRbfPayload(ChannelId channelId) : IMessagePayload
{
    /// <summary>
    /// Gets the channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<TxAckRbfPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            return new TxAckRbfPayload(channelId);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TxAckRbfPayload", e);
        }
    }
}