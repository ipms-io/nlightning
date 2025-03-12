using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

/// <summary>
/// Represents a transaction output.
/// </summary>
public abstract class OutputBase
{
    /// <summary>
    /// Gets the amount of the output in satoshis.
    /// </summary>
    public ulong AmountSats { get; }

    /// <summary>
    /// Gets the scriptPubKey of the output.
    /// </summary>
    public Script ScriptPubKey { get; }

    /// <summary>
    /// Gets the witness stack for Segwit outputs.
    /// </summary>
    public List<ReadOnlyMemory<byte>> WitnessStack { get; }
    
    protected OutputBase(Script scriptPubKey, ulong amountSats, List<ReadOnlyMemory<byte>>? witnessStack = null)
    {
        AmountSats = amountSats;
        ScriptPubKey = scriptPubKey;
        WitnessStack = witnessStack ?? [];
    }
    
    /// <summary>
    /// Converts the output to a NBitcoin.TxOut.
    /// </summary>
    /// <returns>TxOut object.</returns>
    public TxOut ToTxOut()
    {
        return new TxOut((Money)AmountSats, ScriptPubKey);
    }
}