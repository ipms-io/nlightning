using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Constants;
using Interfaces;
using Messages;

/// <summary>
/// Represents a tx_add_input payload.
/// </summary>
/// <remarks>
/// The tx_add_input payload is used to add an input to the transaction.
/// </remarks>
/// <seealso cref="TxAddInputMessage"/>
/// <seealso cref="Channels.ValueObjects.ChannelId"/>
public class TxAddInputPayload : IChannelMessagePayload
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
        if (sequence > InteractiveTransactionConstants.MaxSequence)
        {
            throw new ArgumentException($"Sequence must be less than or equal to {InteractiveTransactionConstants.MaxSequence}", nameof(sequence));
        }

        ChannelId = channelId;
        SerialId = serialId;
        PrevTx = prevTx;
        PrevTxVout = prevTxVout;
        Sequence = sequence;
    }
}