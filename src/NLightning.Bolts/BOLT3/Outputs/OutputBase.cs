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
    public ulong AmountSats { get; set; }

    /// <summary>
    /// Gets the scriptPubKey of the output.
    /// </summary>
    public Script ScriptPubKey { get; }

    protected OutputBase(Script scriptPubKey, ulong amountSats)
    {
        ArgumentNullException.ThrowIfNull(scriptPubKey);

        AmountSats = amountSats;
        ScriptPubKey = scriptPubKey;
    }

    /// <summary>
    /// Converts the output to a NBitcoin.TxOut.
    /// </summary>
    /// <returns>TxOut object.</returns>
    public TxOut ToTxOut()
    {
        return new TxOut((Money)AmountSats, ScriptPubKey);
    }

    public int CompareTo(OutputBase? other) => other is null ? 1 : TransactionOutputComparer.Instance.Compare(this, other);
}