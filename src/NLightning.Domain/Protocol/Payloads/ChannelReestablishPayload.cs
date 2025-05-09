using NBitcoin;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

/// <summary>
/// Represents the payload for the channel_reestablish message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ChannelReestablishPayload class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
public class ChannelReestablishPayload(ChannelId channelId, ulong nextCommitmentNumber, ulong nextRevocationNumber, ReadOnlyMemory<byte> yourLastPerCommitmentSecret, PubKey myCurrentPerCommitmentPoint) : IMessagePayload
{
    /// <summary>
    /// Gets the channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The commitment transaction counter
    /// </summary>
    public ulong NextCommitmentNumber { get; } = nextCommitmentNumber;

    /// <summary>
    /// The commitment counter it expects for the next revoke and ack message 
    /// </summary>
    public ulong NextRevocationNumber { get; } = nextRevocationNumber;

    /// <summary>
    /// The last per commitment secret received
    /// </summary>
    public ReadOnlyMemory<byte> YourLastPerCommitmentSecret { get; } = yourLastPerCommitmentSecret;

    /// <summary>
    /// The current per commitment point
    /// </summary>
    public PubKey MyCurrentPerCommitmentPoint { get; } = myCurrentPerCommitmentPoint;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(NextCommitmentNumber));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(NextRevocationNumber));
        await stream.WriteAsync(YourLastPerCommitmentSecret);
        await stream.WriteAsync(MyCurrentPerCommitmentPoint.ToBytes());
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<ChannelReestablishPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var buffer = new byte[sizeof(ulong)];
            await stream.ReadExactlyAsync(buffer);
            var nextCommitmentNumber = EndianBitConverter.ToUInt64BigEndian(buffer);

            await stream.ReadExactlyAsync(buffer);
            var nextRevocationNumber = EndianBitConverter.ToUInt64BigEndian(buffer);

            var yourLastPerCommitmentSecret = new byte[32];
            await stream.ReadExactlyAsync(yourLastPerCommitmentSecret);

            buffer = new byte[33];
            await stream.ReadExactlyAsync(buffer);
            var myCurrentPerCommitmentPoint = new PubKey(buffer);

            return new ChannelReestablishPayload(channelId, nextCommitmentNumber, nextRevocationNumber, yourLastPerCommitmentSecret, myCurrentPerCommitmentPoint);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing ChannelReestablishPayload", e);
        }
    }
}