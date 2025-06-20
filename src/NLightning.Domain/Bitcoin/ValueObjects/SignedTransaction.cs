namespace NLightning.Domain.Bitcoin.ValueObjects;

using Crypto.ValueObjects;

/// <summary>
/// Represents a fully signed Bitcoin transaction in a domain-agnostic way.
/// </summary>
public record SignedTransaction
{
    public TxId TxId { get; set; }
    public byte[] RawTxBytes { get; set; }

    public ICollection<CompactSignature>? Signatures { get; set; }

    public SignedTransaction(TxId txId, byte[] rawTxBytes, ICollection<CompactSignature>? signatures = null)
    {
        ArgumentNullException.ThrowIfNull(rawTxBytes);
        if (rawTxBytes.Length == 0)
            throw new ArgumentException("Raw transaction bytes cannot be empty.", nameof(rawTxBytes));

        TxId = txId;
        RawTxBytes = rawTxBytes;
        Signatures = signatures;
    }
}