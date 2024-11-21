using NLightning.Common.Constants;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Exceptions;
using Interfaces;

/// <summary>
/// Represents the payload for the update_fail_malformed_htlc message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UpdateFailMalformedHtlcPayload class.
/// </remarks>
public class UpdateFailMalformedHtlcPayload(ChannelId channelId, ulong id, ReadOnlyMemory<byte> sha256OfOnion, ushort failureCode) : IMessagePayload
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
    /// The sha256 of onion if an onion was received
    /// </summary>
    /// <remarks>
    /// May use an all zero array
    /// </remarks>
    public ReadOnlyMemory<byte> Sha256OfOnion { get; } = sha256OfOnion;

    /// <summary>
    /// The failure code
    /// </summary>
    public ushort FailureCode => failureCode;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Id));
        await stream.WriteAsync(Sha256OfOnion);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FailureCode));
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<UpdateFailMalformedHtlcPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var buffer = new byte[sizeof(ulong)];
            await stream.ReadExactlyAsync(buffer);
            var id = EndianBitConverter.ToUInt64BigEndian(buffer);

            var sha256OfOnion = new byte[CryptoConstants.SHA256_HASH_LEN];
            await stream.ReadExactlyAsync(sha256OfOnion);

            buffer = new byte[sizeof(ushort)];
            await stream.ReadExactlyAsync(buffer);
            var failureCode = EndianBitConverter.ToUInt16BigEndian(buffer);

            return new UpdateFailMalformedHtlcPayload(channelId, id, sha256OfOnion, failureCode);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing UpdateFailMalformedHtlcPayload", e);
        }
    }
}