namespace NLightning.Bolts.BOLT3.Transactions;

/// <summary>
/// Represents a transaction output.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TransactionOutput class.
/// </remarks>
/// <param name="value">The value of the output in satoshis.</param>
/// <param name="scriptPubKey">The scriptPubKey of the output.</param>
/// <param name="cltvExpiry">The CLTV expiry for HTLC outputs.</param>
/// <param name="witnessStack">The witness stack for Segwit outputs.</param>
public class TransactionOutput(ulong value, byte[] scriptPubKey, ulong cltvExpiry, List<byte[]>? witnessStack = null)
{
    /// <summary>
    /// Gets the value of the output in satoshis.
    /// </summary>
    public ulong Value { get; } = value;

    /// <summary>
    /// Gets the scriptPubKey of the output.
    /// </summary>
    public byte[] ScriptPubKey { get; } = scriptPubKey;

    /// <summary>
    /// Gets the CLTV expiry for HTLC outputs.
    /// </summary>
    public ulong CltvExpiry { get; } = cltvExpiry;

    /// <summary>
    /// Gets the witness stack for Segwit outputs.
    /// </summary>
    public List<byte[]> WitnessStack { get; } = witnessStack ?? [];
}