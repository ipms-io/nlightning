using System.Runtime.Serialization;
using NBitcoin.Crypto;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Interfaces;

/// <summary>
/// Represents the payload for the closing_signed message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ClosingSignedPayload class.
/// </remarks>
public class ClosingSignedPayload : IMessagePayload
{
    /// <summary>
    /// The channel_id is used to identify this channel.
    /// </summary>
    public ChannelId ChannelId { get; set; }

    /// <summary>
    /// funding_satoshis is the amount the acceptor is putting into the channel.
    /// </summary>
    public ulong FeeSatoshis { get; set; }

    /// <summary>
    /// The signature for the closing transaction
    /// </summary>
    public ECDSASignature Signature { get; }

    public ClosingSignedPayload(ChannelId channelId, ulong feeSatoshis, ECDSASignature signature)
    {
        ChannelId = channelId;
        FeeSatoshis = feeSatoshis;
        Signature = signature;
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FeeSatoshis));
        await stream.WriteAsync(Signature.ToCompact());
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<ClosingSignedPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[sizeof(ulong)];
            await stream.ReadExactlyAsync(bytes);
            var feeSatoshis = EndianBitConverter.ToUInt64BigEndian(bytes);

            bytes = new byte[64];
            await stream.ReadExactlyAsync(bytes);
            if (!ECDSASignature.TryParseFromCompact(bytes, out var signature))
            {
                throw new Exception("Unable to parse signature");
            }

            return new ClosingSignedPayload(channelId, feeSatoshis, signature);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing ClosingSignedPayload", e);
        }
    }
}