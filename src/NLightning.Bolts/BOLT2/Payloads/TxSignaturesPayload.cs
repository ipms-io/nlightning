using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Interfaces;

/// <summary>
/// Represents a tx_signatures payload.
/// </summary>
/// <remarks>
/// The tx_signatures payload signals the provision of transaction signatures.
/// </remarks>
/// <seealso cref="Messages.TxSignaturesMessage"/>
/// <seealso cref="Common.Types.ChannelId"/>
/// <seealso cref="Witness"/>
public class TxSignaturesPayload : IMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    public ChannelId ChannelId { get; }

    /// <summary>
    /// The transaction id.
    /// </summary>
    public byte[] TxId { get; }

    /// <summary>
    /// The witnesses.
    /// </summary>
    public List<Witness> Witnesses { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TxSignaturesPayload"/> class.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="txId">The transaction id.</param>
    /// <param name="witnesses">The witnesses.</param>
    /// <exception cref="ArgumentException">TxId must be 32 bytes</exception>
    public TxSignaturesPayload(ChannelId channelId, byte[] txId, List<Witness> witnesses)
    {
        if (txId.Length != 32)
        {
            throw new ArgumentException("TxId must be 32 bytes", nameof(txId));
        }

        ChannelId = channelId;
        TxId = txId;
        Witnesses = witnesses;
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(TxId);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)Witnesses.Count));

        foreach (var witness in Witnesses)
        {
            await witness.SerializeAsync(stream);
        }
    }

    /// <summary>
    /// Deserialize a TxSignaturesPayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxSignaturesPayload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<TxSignaturesPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var txId = new byte[32];
            await stream.ReadExactlyAsync(txId);

            var bytes = new byte[2];
            await stream.ReadExactlyAsync(bytes);
            var numWitnesses = EndianBitConverter.ToUInt16BigEndian(bytes);

            var witnesses = new List<Witness>(numWitnesses);
            for (var i = 0; i < numWitnesses; i++)
            {
                witnesses.Add(await Witness.DeserializeAsync(stream));
            }

            return new TxSignaturesPayload(channelId, txId, witnesses);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TxSignaturesPayload", e);
        }
    }
}