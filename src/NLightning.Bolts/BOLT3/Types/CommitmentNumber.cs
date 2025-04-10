using NBitcoin;

namespace NLightning.Bolts.BOLT3.Types;

using Common.Crypto.Hashes;

/// <summary>
/// Manages Lightning Network commitment numbers and their obscuring as defined in BOLT3.
/// </summary>
public sealed class CommitmentNumber
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
    /// Initializes a new instance of the <see cref="CommitmentNumber"/> class.
    /// </summary>
    /// <param name="localPaymentBasepoint">The local payment basepoint.</param>
    /// <param name="remotePaymentBasepoint">The remote payment basepoint.</param>
    /// <param name="initialValue">The initial commitment number. Defaults to 0</param>
    public CommitmentNumber(PubKey localPaymentBasepoint, PubKey remotePaymentBasepoint, ulong initialValue = 0)
    {
        ArgumentNullException.ThrowIfNull(localPaymentBasepoint);
        ArgumentNullException.ThrowIfNull(remotePaymentBasepoint);

        Value = initialValue;
        ObscuringFactor = CalculateObscuringFactor(localPaymentBasepoint, remotePaymentBasepoint);
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
    public LockTime CalculateLockTime()
    {
        return new LockTime((0x20 << 24) | (uint)(ObscuredValue & 0xFFFFFF));
    }

    /// <summary>
    /// Calculates the transaction sequence value using the obscured commitment number.
    /// </summary>
    /// <returns>The transaction sequence.</returns>
    public Sequence CalculateSequence()
    {
        return new Sequence((uint)((0x80UL << 24) | ((ObscuredValue >> 24) & 0xFFFFFF)));
    }

    /// <summary>
    /// Calculates the 48-bit obscuring factor by hashing the concatenation of payment basepoints.
    /// </summary>
    /// <param name="localBasepoint">The local payment basepoint.</param>
    /// <param name="remoteBasepoint">The remote payment basepoint.</param>
    /// <returns>The 48-bit obscuring factor as a ulong.</returns>
    private static ulong CalculateObscuringFactor(PubKey localBasepoint, PubKey remoteBasepoint)
    {
        using var sha256 = new Sha256();

        // Hash the concatenation of payment basepoints
        sha256.AppendData(localBasepoint.ToBytes());
        sha256.AppendData(remoteBasepoint.ToBytes());

        Span<byte> hashResult = stackalloc byte[32];
        sha256.GetHashAndReset(hashResult);

        // Extract the lower 48 bits (6 bytes) of the hash
        ulong obscuringFactor = 0;
        for (var i = 26; i < 32; i++) // Last 6 bytes of the 32-byte hash
        {
            obscuringFactor = (obscuringFactor << 8) | hashResult[i];
        }

        return obscuringFactor;
    }
}