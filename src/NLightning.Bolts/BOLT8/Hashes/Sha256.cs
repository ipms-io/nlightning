using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NLightning.Bolts.BOLT8.Hashes;

using Constants;

/// <summary>
/// SHA-256 from <see href="https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.180-4.pdf">FIPS 180-4</see>.
/// </summary>
internal sealed class Sha256 : IDisposable
{
    private readonly IntPtr _state = Marshal.AllocHGlobal(104);
    private bool _disposed;

    internal Sha256() => Reset();

    /// <summary>
    /// Appends the specified data to the data already processed in the hash.
    /// </summary>
    internal void AppendData(ReadOnlySpan<byte> data)
    {
        if (!data.IsEmpty)
        {
            _ = Libsodium.crypto_hash_sha256_update(
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
        Debug.Assert(hash.Length == HashConstants.HASH_LEN);

        _ = Libsodium.crypto_hash_sha256_final(
            _state,
            ref MemoryMarshal.GetReference(hash)
        );

        Reset();
    }

    private void Reset()
    {
        _ = Libsodium.crypto_hash_sha256_init(_state);
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

    ~Sha256()
    {
        Dispose();
    }
    #endregion
}