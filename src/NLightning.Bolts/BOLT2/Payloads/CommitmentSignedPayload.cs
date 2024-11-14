using System.Runtime.Serialization;
using NBitcoin.Crypto;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Interfaces;

public class CommitmentSignedPayload : IMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; set; }

    /// <summary>
    /// The signature for the commitment transaction
    /// </summary>
    public ECDSASignature Signature { get; set; }

    /// <summary>
    /// Number of HTLCs outputs
    /// </summary>
    public ushort NumHtlcs { get; set; }

    /// <summary>
    /// List containing HTLCs signatures
    /// </summary>
    public IEnumerable<ECDSASignature> HtlcSignatures { get; set; }

    public CommitmentSignedPayload(ChannelId channelId, ECDSASignature signature, IEnumerable<ECDSASignature> htlcSignatures)
    {
        ChannelId = channelId;
        Signature = signature;
        NumHtlcs = (ushort)htlcSignatures.Count();
        HtlcSignatures = htlcSignatures;
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(Signature.ToCompact());
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(NumHtlcs));
        foreach (var htlcsSignature in HtlcSignatures)
        {
            await stream.WriteAsync(htlcsSignature.ToCompact());
        }
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<CommitmentSignedPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[64];
            await stream.ReadExactlyAsync(bytes);
            if (!ECDSASignature.TryParseFromCompact(bytes, out var signature))
            {
                throw new Exception("Unable to parse signature");
            }

            bytes = new byte[sizeof(ushort)];
            await stream.ReadExactlyAsync(bytes);
            var numHtlcs = EndianBitConverter.ToUInt16BigEndian(bytes);

            var htlcSignatures = new List<ECDSASignature>(numHtlcs);
            for (var i = 0; i < numHtlcs; i++)
            {
                bytes = new byte[64];
                await stream.ReadExactlyAsync(bytes);
                if (!ECDSASignature.TryParseFromCompact(bytes, out var htlcSignature))
                {
                    throw new Exception("Unable to parse signature");
                }

                htlcSignatures.Add(htlcSignature);
            }

            return new CommitmentSignedPayload(channelId, signature, htlcSignatures);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing CommitmentSignedPayload", e);
        }
    }
}