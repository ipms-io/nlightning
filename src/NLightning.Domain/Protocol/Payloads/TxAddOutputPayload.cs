namespace NLightning.Domain.Protocol.Payloads;

using Bitcoin.ValueObjects;
using Channels.ValueObjects;
using Interfaces;
using Messages;
using Money;

/// <summary>
/// Represents a tx_add_output payload.
/// </summary>
/// <remarks>
/// The tx_add_output payload is used to add an output to the transaction.
/// </remarks>
/// <seealso cref="TxAddOutputMessage"/>
/// <seealso cref="Channels.ValueObjects.ChannelId"/>
public class TxAddOutputPayload : IChannelMessagePayload
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
    /// The sats amount.
    /// </summary>
    public LightningMoney Amount { get; }

    /// <summary>
    /// The spending script.
    /// </summary>
    public BitcoinScript Script { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TxAddOutputPayload"/> class.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="serialId">The serial id.</param>
    /// <param name="amount">The sats amount.</param>
    /// <param name="script">The spending script.</param>
    /// <exception cref="ArgumentException">ScriptPubKey length is out of bounds.</exception>
    public TxAddOutputPayload(LightningMoney amount, ChannelId channelId, BitcoinScript script, ulong serialId)
    {
        ChannelId = channelId;
        SerialId = serialId;
        Amount = amount;
        Script = script;
    }
}