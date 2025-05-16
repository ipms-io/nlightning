using NBitcoin;

namespace NLightning.Infrastructure.Protocol.Outputs;

using Comparers;
using Domain.Money;

/// <summary>
/// Represents a transaction output.
/// </summary>
public abstract class BaseOutput
{
    /// <summary>
    /// Gets the amount of the output in satoshis.
    /// </summary>
    public LightningMoney Amount { get; set; }

    /// <summary>
    /// Gets the scriptPubKey of the output.
    /// </summary>
    public Script ScriptPubKey { get; }

    /// <summary>
    /// Gets or sets the transaction ID of the output.
    /// </summary>
    public uint256? TxId { get; set; }

    /// <summary>
    /// Gets or sets the index of the output in the transaction.
    /// </summary>
    /// <remarks>
    /// Output is nonexistent if this is -1.
    /// </remarks>
    public int Index { get; set; }

    public Script RedeemScript { get; }

    public abstract ScriptType ScriptType { get; }

    protected BaseOutput(Script redeemScript, Script scriptPubKey, LightningMoney amount)
    {
        ArgumentNullException.ThrowIfNull(redeemScript);
        ArgumentNullException.ThrowIfNull(scriptPubKey);
        ArgumentNullException.ThrowIfNull(amount);

        Amount = amount;
        RedeemScript = redeemScript;
        ScriptPubKey = scriptPubKey;
        TxId = uint256.Zero;
        Index = -1;
    }
    protected BaseOutput(Script redeemScript, LightningMoney amount)
    {
        ArgumentNullException.ThrowIfNull(redeemScript);
        ArgumentNullException.ThrowIfNull(amount);

        Amount = amount;
        RedeemScript = redeemScript;
        ScriptPubKey = ScriptType switch
        {
            ScriptType.P2WPKH
                or ScriptType.P2WSH
                when redeemScript.ToString().StartsWith("0 ") => redeemScript,
            ScriptType.P2WPKH
                or ScriptType.P2WSH => redeemScript.WitHash.ScriptPubKey,
            ScriptType.P2SH => redeemScript.Hash.ScriptPubKey,
            _ => redeemScript.PaymentScript
        };
        TxId = uint256.Zero;
        Index = -1;
    }

    /// <summary>
    /// Converts the output to a NBitcoin.TxOut.
    /// </summary>
    /// <returns>TxOut object.</returns>
    public TxOut ToTxOut()
    {
        return new TxOut((Money)Amount, ScriptPubKey);
    }

    public ScriptCoin ToCoin()
    {
        if (Index == -1)
            throw new InvalidOperationException("Output is nonexistent. Sign the transaction first.");

        if (TxId is null || TxId == uint256.Zero || TxId == uint256.One)
            throw new InvalidOperationException("Transaction ID is not set. Sign the transaction first.");

        if (Amount.IsZero)
            throw new InvalidOperationException("You can't spend a zero amount output.");

        return new ScriptCoin(TxId, checked((uint)Index), Amount, ScriptPubKey, RedeemScript);
    }

    public int CompareTo(BaseOutput? other) => other is null ? 1 : TransactionOutputComparer.Instance.Compare(this, other);
}