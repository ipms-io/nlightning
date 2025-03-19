using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

using Comparers;

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

    public uint Index { get; set; }

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
    }
    protected BaseOutput(Script redeemScript, LightningMoney amount)
    {
        ArgumentNullException.ThrowIfNull(redeemScript);
        ArgumentNullException.ThrowIfNull(amount);
        // 0 77abe0c6e4735b7a9858dc82bb7ec4e6889532e356607095f5ba685b58a7f9ab
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
    }

    /// <summary>
    /// Converts the output to a NBitcoin.TxOut.
    /// </summary>
    /// <returns>TxOut object.</returns>
    public TxOut ToTxOut()
    {
        return new TxOut((Money)Amount, ScriptPubKey);
    }

    public virtual ScriptCoin ToCoin()
    {
        if (TxId is null || TxId == uint256.Zero || TxId == uint256.One)
            throw new InvalidOperationException("Transaction ID is not set. Sign the transaction first.");

        if (Amount.IsZero)
            throw new InvalidOperationException("You can't spend a zero amount output.");

        return new ScriptCoin(TxId, Index, Amount, ScriptPubKey, RedeemScript);
    }

    public int CompareTo(BaseOutput? other) => other is null ? 1 : TransactionOutputComparer.Instance.Compare(this, other);
}