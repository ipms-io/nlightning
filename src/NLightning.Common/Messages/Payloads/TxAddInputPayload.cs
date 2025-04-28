namespace NLightning.Common.Messages.Payloads;

using BitUtils;
using Constants;
using Exceptions;
using Interfaces;
using Types;

/// <summary>
/// Represents a tx_add_input payload.
/// </summary>
/// <remarks>
/// The tx_add_input payload is used to add an input to the transaction.
/// </remarks>
/// <seealso cref="TxAddInputMessage"/>
/// <seealso cref="Common.Types.ChannelId"/>
public class TxAddInputPayload : IMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    public ChannelId ChannelId { get; }

    /// <summary>
    /// The serial id.
    /// </summary>
    public ulong SerialId { get; }

    /// <summary>
    /// The previous transaction id.
    /// </summary>
    public byte[] PrevTx { get; }

    /// <summary>
    /// The previous transaction vout.
    /// </summary>
    public uint PrevTxVout { get; }

    /// <summary>
    /// The sequence.
    /// </summary>
    public uint Sequence { get; }

    /// <summary>
    /// Initializes a new instance of the TxAddInputPayload class.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="serialId">The serial id.</param>
    /// <param name="prevTx">The previous transaction id.</param>
    /// <param name="prevTxVout">The previous transaction vout.</param>
    /// <param name="sequence">The sequence.</param>
    /// <exception cref="ArgumentException">Sequence is out of bounds.</exception>
    public TxAddInputPayload(ChannelId channelId, ulong serialId, byte[] prevTx, uint prevTxVout, uint sequence)
    {
        if (sequence > InteractiveTransactionContants.MAX_SEQUENCE)
        {
            throw new ArgumentException($"Sequence must be less than or equal to {InteractiveTransactionContants.MAX_SEQUENCE}", nameof(sequence));
        }

        ChannelId = channelId;
        SerialId = serialId;
        PrevTx = prevTx;
        PrevTxVout = prevTxVout;
        Sequence = sequence;
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(SerialId));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)PrevTx.Length));
        await stream.WriteAsync(PrevTx);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(PrevTxVout));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Sequence));
    }

    /// <summary>
    /// Deserialize a TxAddInputPayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxAddInputPayload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<TxAddInputPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            var serialId = EndianBitConverter.ToUInt64BigEndian(bytes);

            bytes = new byte[2];
            await stream.ReadExactlyAsync(bytes);
            var prevTxLength = EndianBitConverter.ToUInt16BigEndian(bytes);

            var prevTx = new byte[prevTxLength];
            await stream.ReadExactlyAsync(prevTx);

            bytes = new byte[4];
            await stream.ReadExactlyAsync(bytes);
            var prevTxVout = EndianBitConverter.ToUInt32BigEndian(bytes);

            bytes = new byte[4];
            await stream.ReadExactlyAsync(bytes);
            var sequence = EndianBitConverter.ToUInt32BigEndian(bytes);

            return new TxAddInputPayload(channelId, serialId, prevTx, prevTxVout, sequence);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing TxAddInputPayload", e);
        }
    }
}