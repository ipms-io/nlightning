using System.Buffers;
using NBitcoin.Crypto;

namespace NLightning.Common.Messages.Payloads;

using Exceptions;
using Interfaces;
using Types;

/// <summary>
/// Represents the payload for the funding_created message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the FundingCreatedPayload class.
/// </remarks>
public class FundingSignedPayload : IMessagePayload
{
    /// <summary>
    /// The channel_id is used to identify this channel on a per-peer basis until the funding transaction
    /// is established, at which point it is replaced by the channel_id, which is derived from the funding transaction.
    /// </summary>
    public ChannelId ChannelId { get; set; }

    /// <summary>
    /// The signature of the funding transaction.
    /// </summary>
    public ECDSASignature Signature { get; set; }

    public FundingSignedPayload(ChannelId channelId, ECDSASignature signature)
    {
        ChannelId = channelId;
        Signature = signature;
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(Signature.ToCompact());
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<FundingSignedPayload> DeserializeAsync(Stream stream)
    {
        var bytes = ArrayPool<byte>.Shared.Rent(64);

        try
        {
            var temporaryChannelId = await ChannelId.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..64]);
            if (!ECDSASignature.TryParseFromCompact(bytes[..64], out var signature))
            {
                throw new Exception("Unable to parse signature");
            }

            return new FundingSignedPayload(temporaryChannelId, signature);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing FundingSignedPayload", e);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }
}