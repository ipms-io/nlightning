using NBitcoin;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.Interfaces;

/// <summary>
/// Represents the payload for the channel_ready message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ChannelReadyPayload class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
public class ChannelReadyPayload(ChannelId channelId, PubKey secondPerCommitmentPoint) : IMessagePayload
{
    /// <summary>
    /// Gets the channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    public PubKey SecondPerCommitmentPoint { get; } = secondPerCommitmentPoint;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(SecondPerCommitmentPoint.ToBytes());
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<ChannelReadyPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var buffer = new byte[33];
            await stream.ReadExactlyAsync(buffer);
            var secondPerCommitmentPoint = new PubKey(buffer);

            return new ChannelReadyPayload(channelId, secondPerCommitmentPoint);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing ChannelReadyPayload", e);
        }
    }
}