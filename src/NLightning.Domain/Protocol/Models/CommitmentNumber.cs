namespace NLightning.Domain.Protocol.Models;

using Bitcoin.ValueObjects;
using Crypto.Hashes;
using Crypto.ValueObjects;

/// <summary>
/// Manages Lightning Network commitment numbers and their obscuring as defined in BOLT3.
/// </summary>
public class CommitmentNumber
{
    /// <summary>
    /// Gets the commitment number value.
    /// </summary>
    public ulong Value { get; private set; }

    /// <summary>
    /// Gets the obscuring factor derived from payment basepoints.
    /// </summary>
    public ulong ObscuringFactor { get; }

    /// <summary>
    /// Gets the obscured commitment number (value XOR obscuring factor).
    /// </summary>
    public ulong ObscuredValue => Value ^ ObscuringFactor;

    /// <summary>
    /// Represents a commitment number in the Lightning Network.
    /// </summary>
    public CommitmentNumber(CompactPubKey localPaymentBasepoint, CompactPubKey remotePaymentBasepoint, ISha256 sha256,
                            ulong initialValue = 0)
    {
        Value = initialValue;
        ObscuringFactor = CalculateObscuringFactor(localPaymentBasepoint, remotePaymentBasepoint, sha256);
    }

    /// <summary>
    /// Increments the commitment number.
    /// </summary>
    /// <returns>This instance for chaining.</returns>
    public CommitmentNumber Increment()
    {
        Value++;
        return this;
    }

    /// <summary>
    /// Calculates the transaction locktime value using the obscured commitment number.
    /// </summary>
    /// <returns>The transaction locktime.</returns>
    public BitcoinLockTime CalculateLockTime()
    {
        return new BitcoinLockTime((0x20 << 24) | (uint)(ObscuredValue & 0xFFFFFF));
    }

    /// <summary>
    /// Calculates the transaction sequence value using the obscured commitment number.
    /// </summary>
    /// <returns>The transaction sequence.</returns>
    public BitcoinSequence CalculateSequence()
    {
        return new BitcoinSequence((uint)((0x80UL << 24) | ((ObscuredValue >> 24) & 0xFFFFFF)));
    }

    /// <summary>
    /// Calculates the 48-bit obscuring factor by hashing the concatenation of payment basepoints.
    /// </summary>
    /// <param name="localBasepoint">The local payment basepoint.</param>
    /// <param name="remoteBasepoint">The remote payment basepoint.</param>
    /// <param name="sha256">The SHA256 hash function instance.</param>
    /// <returns>The 48-bit obscuring factor as ulong.</returns>
    private static ulong CalculateObscuringFactor(CompactPubKey localBasepoint, CompactPubKey remoteBasepoint,
                                                  ISha256 sha256)
    {
        // Hash the concatenation of payment basepoints
        sha256.AppendData(localBasepoint);
        sha256.AppendData(remoteBasepoint);

        Span<byte> hashResult = stackalloc byte[32];
        sha256.GetHashAndReset(hashResult);

        // Extract the lower 48 bits (6 bytes) of the hash
        ulong obscuringFactor = 0;
        for (var i = 26; i < 32; i++) // Last 6 bytes of the 32-byte hash
            obscuringFactor = (obscuringFactor << 8) | hashResult[i];

        return obscuringFactor;
    }
}