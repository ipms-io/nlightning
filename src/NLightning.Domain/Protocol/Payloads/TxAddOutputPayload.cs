using NBitcoin;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Messages;
using Money;
using ValueObjects;

/// <summary>
/// Represents a tx_add_output payload.
/// </summary>
/// <remarks>
/// The tx_add_output payload is used to add an output to the transaction.
/// </remarks>
/// <seealso cref="TxAddOutputMessage"/>
/// <seealso cref="ValueObjects.ChannelId"/>
public class TxAddOutputPayload : IMessagePayload
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
    public Script Script { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TxAddOutputPayload"/> class.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="serialId">The serial id.</param>
    /// <param name="amount">The sats amount.</param>
    /// <param name="script">The spending script.</param>
    /// <exception cref="ArgumentException">ScriptPubKey length is out of bounds.</exception>
    public TxAddOutputPayload(LightningMoney amount, ChannelId channelId, Script script, ulong serialId)
    {
        // Check if script is only types P2WSH, P2WPKH, or P2TR using NBitcoin
        if (!PayToWitScriptHashTemplate.Instance.CheckScriptPubKey(script)
            && !PayToWitPubKeyHashTemplate.Instance.CheckScriptPubKey(script)
            && !PayToTaprootTemplate.Instance.CheckScriptPubKey(script))
        {
            throw new ArgumentException("Script is non-standard");
        }

        ChannelId = channelId;
        SerialId = serialId;
        Amount = amount;
        Script = script;
    }
}