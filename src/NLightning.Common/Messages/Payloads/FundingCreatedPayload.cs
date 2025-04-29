using System.Buffers;
using NBitcoin.Crypto;

namespace NLightning.Common.Messages.Payloads;

using BitUtils;
using Constants;
using Exceptions;
using Interfaces;
using Types;

/// <summary>
/// Represents the payload for the funding_created message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the FundingCreatedPayload class.
/// </remarks>
public class FundingCreatedPayload : IMessagePayload
{
    /// <summary>
    /// The temporary_channel_id is used to identify this channel on a per-peer basis until the funding transaction
    /// is established, at which point it is replaced by the channel_id, which is derived from the funding transaction.
    /// </summary>
    public ChannelId TemporaryChannelId { get; set; }

    /// <summary>
    /// The funding transaction id.
    /// </summary>
    public ReadOnlyMemory<byte> FundingTxId { get; set; }

    /// <summary>
    /// The funding transaction output index.
    /// </summary>
    public ushort FundingOutputIndex { get; set; }

    /// <summary>
    /// The signature of the funding transaction.
    /// </summary>
    public ECDSASignature Signature { get; set; }

    public FundingCreatedPayload(ChannelId temporaryChannelId, ReadOnlySpan<byte> fundingTxId, ushort fundingOutputIndex,
                                 ECDSASignature signature)
    {
        TemporaryChannelId = temporaryChannelId;
        FundingTxId = fundingTxId.ToArray();
        FundingOutputIndex = fundingOutputIndex;
        Signature = signature;
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await TemporaryChannelId.SerializeAsync(stream);
        await stream.WriteAsync(FundingTxId);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FundingOutputIndex));
        await stream.WriteAsync(Signature.ToCompact());
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<FundingCreatedPayload> DeserializeAsync(Stream stream)
    {
        var bytes = ArrayPool<byte>.Shared.Rent(64);

        try
        {
            var temporaryChannelId = await ChannelId.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..HashConstants.SHA256_HASH_LEN]);
            var fundingTxId = bytes[..HashConstants.SHA256_HASH_LEN];

            await stream.ReadExactlyAsync(bytes.AsMemory()[..sizeof(ushort)]);
            var fundingOutputIndex = EndianBitConverter.ToUInt16BigEndian(bytes[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..64]);
            if (!ECDSASignature.TryParseFromCompact(bytes[..64], out var signature))
            {
                throw new Exception("Unable to parse signature");
            }

            return new FundingCreatedPayload(temporaryChannelId, fundingTxId, fundingOutputIndex, signature);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing FundingCreatedPayload", e);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }
}