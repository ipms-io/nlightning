using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

using Comparers;

/// <summary>
/// Represents a transaction output.
/// </summary>
public abstract class OutputBase
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

    protected OutputBase(Script redeemScript, Script scriptPubKey, LightningMoney amount)
    {
        ArgumentNullException.ThrowIfNull(redeemScript);
        ArgumentNullException.ThrowIfNull(scriptPubKey);
        ArgumentNullException.ThrowIfNull(amount);

        Amount = amount;
        RedeemScript = redeemScript;
        ScriptPubKey = scriptPubKey;
    }
    protected OutputBase(Script redeemScript, LightningMoney amount)
    {
        ArgumentNullException.ThrowIfNull(redeemScript);
        ArgumentNullException.ThrowIfNull(amount);

        Amount = amount;
        RedeemScript = redeemScript;
        ScriptPubKey = redeemScript.WitHash.ScriptPubKey;
    }

    /// <summary>
    /// Converts the output to a NBitcoin.TxOut.
    /// </summary>
    /// <returns>TxOut object.</returns>
    public TxOut ToTxOut()
    {
        return new TxOut((Money)Amount, ScriptPubKey);
    }

    public virtual Coin ToCoin()
    {
        if (TxId is null || TxId == uint256.Zero || TxId == uint256.One)
            throw new InvalidOperationException("Transaction ID is not set. Sign the transaction first.");

        if (Amount.IsZero)
            throw new InvalidOperationException("You can't spend a zero amount output.");

        return new Coin(TxId, Index, Amount, RedeemScript);
    }

    public int CompareTo(OutputBase? other) => other is null ? 1 : TransactionOutputComparer.Instance.Compare(this, other);
}