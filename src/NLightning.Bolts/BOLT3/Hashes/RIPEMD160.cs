using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NLightning.Bolts.BOLT3.Hashes;

using BOLT8;
using Bolts.Constants;

/// <summary>
/// RIPEMD-160 hash function implementation based on the algorithm described in RFC 2925 (https://tools.ietf.org/html/rfc2925).
/// </summary>
internal sealed class RIPEMD160 : IDisposable
{
    private readonly IntPtr _state = Marshal.AllocHGlobal(80); // RIPEMD-160 uses 80 bytes for its internal state
    private bool _disposed;

    internal RIPEMD160() => Reset();

    /// <summary>
    /// Appends the specified data to the data already processed in the hash.
    /// </summary>
    internal void AppendData(ReadOnlySpan<byte> data)
    {
        if (!data.IsEmpty)
        {
            // Assuming Libsodium has a function similar to crypto_hash_sha256_update for RIPEMD-160, adjust the method call accordingly
            _ = Libsodium.crypto_hash_ripemd160_update(
                _state,
                ref MemoryMarshal.GetReference(data),
                (ulong)data.Length
            );
        }
    }

    /// <summary>
    /// Retrieves the hash for the accumulated data into the hash parameter,
    /// and resets the object to its initial state.
    /// </summary>
    internal void GetHashAndReset(Span<byte> hash)
    {
        Debug.Assert(hash.Length == HashConstants.RIPEMD160_HASH_LEN);

        // Assuming Libsodium has a function similar to crypto_hash_sha256_final for RIPEMD-160, adjust the method call accordingly
        _ = Libsodium.crypto_hash_ripemd160_final(
            _state,
            ref MemoryMarshal.GetReference(hash)
        );

        Reset();
    }

    private void Reset()
    {
        // Assuming Libsodium has an initialization function for RIPEMD-160
        _ = Libsodium.crypto_hash_ripemd160_init(_state);
    }

    #region Dispose Pattern
    public void Dispose()
    {
        if (!_disposed)
        {
            Marshal.FreeHGlobal(_state);
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    ~RIPEMD160()
    {
        Dispose();
    }
    #endregion
}