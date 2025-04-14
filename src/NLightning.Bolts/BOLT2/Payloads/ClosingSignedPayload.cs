using NBitcoin.Crypto;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Common.Enums;
using Common.Interfaces;

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
    public LightningMoney FeeAmount { get; set; }

    /// <summary>
    /// The signature for the closing transaction
    /// </summary>
    public ECDSASignature Signature { get; }

    public ClosingSignedPayload(ChannelId channelId, LightningMoney feeAmount, ECDSASignature signature)
    {
        ChannelId = channelId;
        FeeAmount = feeAmount;
        Signature = signature;
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FeeAmount.Satoshi));
        await stream.WriteAsync(Signature.ToCompact());
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<ClosingSignedPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[sizeof(ulong)];
            await stream.ReadExactlyAsync(bytes);
            var feeSatoshis = LightningMoney.FromUnit(EndianBitConverter.ToUInt64BigEndian(bytes),
                                                      LightningMoneyUnit.SATOSHI);

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
            throw new PayloadSerializationException("Error deserializing ClosingSignedPayload", e);
        }
    }
}