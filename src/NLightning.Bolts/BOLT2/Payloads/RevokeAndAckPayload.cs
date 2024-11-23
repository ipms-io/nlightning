using NBitcoin;

namespace NLightning.Bolts.BOLT2.Payloads;

using Exceptions;
using Interfaces;

/// <summary>
/// Represents the payload for the revoke_and_ack message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the RevokeAndAckPayload class.
/// </remarks>
public class RevokeAndAckPayload(ChannelId channelId, ReadOnlyMemory<byte> perCommitmentSecret, PubKey nextPerCommitmentPoint) : IMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// len is the per commitment secret
    /// </summary>
    public ReadOnlyMemory<byte> PerCommitmentSecret { get; } = perCommitmentSecret;

    /// <summary>
    /// The next per commitment point
    /// </summary>
    public PubKey NextPerCommitmentPoint { get; } = nextPerCommitmentPoint;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(PerCommitmentSecret);
        await stream.WriteAsync(NextPerCommitmentPoint.ToBytes());
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<RevokeAndAckPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var perCommitmentSecret = new byte[32];
            await stream.ReadExactlyAsync(perCommitmentSecret);

            var buffer = new byte[33];
            await stream.ReadExactlyAsync(buffer);
            var scriptPubkey = new PubKey(buffer);

            return new RevokeAndAckPayload(channelId, perCommitmentSecret, scriptPubkey);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing RevokeAndAckPayload", e);
        }
    }
}