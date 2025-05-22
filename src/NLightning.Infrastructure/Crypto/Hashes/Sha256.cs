using System.Diagnostics;

namespace NLightning.Infrastructure.Crypto.Hashes;

using Domain.Crypto.Constants;
using Domain.Crypto.Hashes;
using Factories;
using Interfaces;

/// <summary>
/// SHA-256 from <see href="https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.180-4.pdf">FIPS 180-4</see>.
/// </summary>
public sealed class Sha256 : ISha256
{
    private readonly ICryptoProvider _cryptoProvider;
    private readonly IntPtr _state;

    public Sha256()
    {
        _cryptoProvider = CryptoFactory.GetCryptoProvider();
        _state = _cryptoProvider.MemoryAlloc(CryptoConstants.LIBSODIUM_SHA256_STATE_LEN);
        Reset();
    }

    /// <summary>
    /// Appends the specified data to the data already processed in the hash.
    /// </summary>
    public void AppendData(ReadOnlySpan<byte> data)
    {
        if (!data.IsEmpty)
        {
            _cryptoProvider.Sha256Update(_state, data);
        }
    }

    /// <summary>
    /// Retrieves the hash for the accumulated data into the hash parameter,
    /// and resets the object to its initial state.
    /// </summary>
    public void GetHashAndReset(Span<byte> hash)
    {
        Debug.Assert(hash.Length == CryptoConstants.SHA256_HASH_LEN);

        _cryptoProvider.Sha256Final(_state, hash);

        Reset();
    }

    private void Reset()
    {
        _cryptoProvider.Sha256Init(_state);
    }

    #region Dispose Pattern
    private void ReleaseUnmanagedResources()
    {
        _cryptoProvider.MemoryZero(_state, CryptoConstants.SHA256_HASH_LEN);
        _cryptoProvider.MemoryFree(_state);
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            _cryptoProvider.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Sha256()
    {
        Dispose(false);
    }
    #endregion
}