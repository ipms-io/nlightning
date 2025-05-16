using NLightning.Domain.Crypto.Constants;

namespace NLightning.Infrastructure.Transport.Handshake.States;

using Crypto.Factories;
using Crypto.Functions;
using Crypto.Hashes;
using Crypto.Interfaces;
using Crypto.Primitives;

/// <summary>
/// A SymmetricState object contains a CipherState plus ck (a chaining
/// key of HashLen bytes) and h (a hash output of HashLen bytes).
/// </summary>
internal sealed class SymmetricState : IDisposable
{
    private readonly ICryptoProvider _cryptoProvider;
    private readonly Sha256 _sha256 = new();
    private readonly Hkdf _hkdf = new();
    private readonly CipherState _state = new();
    private readonly SecureMemory _ck;
    private readonly byte[] _h;

    private bool _disposed;

    /// <summary>
    /// Initializes a new SymmetricState with an
    /// arbitrary-length protocolName byte sequence.
    /// </summary>
    public SymmetricState(ReadOnlySpan<byte> protocolName)
    {
        _cryptoProvider = CryptoFactory.GetCryptoProvider();
        _ck = new SecureMemory(CryptoConstants.SHA256_HASH_LEN);
        _h = new byte[CryptoConstants.SHA256_HASH_LEN];

        if (protocolName.Length <= CryptoConstants.SHA256_HASH_LEN)
        {
            protocolName.CopyTo(_h);
        }
        else
        {
            _sha256.AppendData(protocolName);
            _sha256.GetHashAndReset(_h);
        }

        _h.CopyTo(_ck);
    }

    /// <summary>
    /// Sets ck, tempK = HKDF(ck, inputKeyMaterial, 2).
    /// If HashLen is 64, then truncates tempK to 32 bytes.
    /// Calls InitializeKey(tempK).
    /// </summary>
    public void MixKey(ReadOnlySpan<byte> inputKeyMaterial)
    {
        ThrowIfDisposed(_disposed, nameof(Hkdf));

        var length = inputKeyMaterial.Length;
        if (length != 0 && length != CryptoConstants.PRIVKEY_LEN)
        {
            throw new ArgumentOutOfRangeException(nameof(inputKeyMaterial), $"Length should be either 0 or {CryptoConstants.PRIVKEY_LEN}");
        }

        Span<byte> output = stackalloc byte[2 * CryptoConstants.SHA256_HASH_LEN];
        _hkdf.ExtractAndExpand2(_ck, inputKeyMaterial, output);

        output[..CryptoConstants.SHA256_HASH_LEN].CopyTo(_ck);

        var tempK = output.Slice(CryptoConstants.SHA256_HASH_LEN, CryptoConstants.PRIVKEY_LEN);
        _state.InitializeKeyAndChainingKey(tempK, _ck);
    }

    private void ThrowIfDisposed(bool disposed, string hkdfName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sets h = HASH(h || data).
    /// </summary>
    public void MixHash(ReadOnlySpan<byte> data)
    {
        ThrowIfDisposed(_disposed, nameof(Hkdf));

        _sha256.AppendData(_h);
        _sha256.AppendData(data);
        _sha256.GetHashAndReset(_h);
    }

    /// <summary>
    /// Sets ck, tempH, tempK = HKDF(ck, inputKeyMaterial, 3).
    /// Calls MixHash(tempH).
    /// If HashLen is 64, then truncates tempK to 32 bytes.
    /// Calls InitializeKey(tempK).
    /// </summary>
    public void MixKeyAndHash(ReadOnlySpan<byte> inputKeyMaterial)
    {
        ThrowIfDisposed(_disposed, nameof(Hkdf));

        var length = inputKeyMaterial.Length;
        if (length != 0 && length != CryptoConstants.PRIVKEY_LEN)
        {
            throw new ArgumentOutOfRangeException(nameof(inputKeyMaterial), $"Length should be either 0 or {CryptoConstants.PRIVKEY_LEN}");
        }

        Span<byte> output = stackalloc byte[3 * CryptoConstants.SHA256_HASH_LEN];
        _hkdf.ExtractAndExpand3(_ck, inputKeyMaterial, output);

        output[..CryptoConstants.SHA256_HASH_LEN].CopyTo(_ck);

        var tempH = output.Slice(CryptoConstants.SHA256_HASH_LEN, CryptoConstants.SHA256_HASH_LEN);
        var tempK = output.Slice(2 * CryptoConstants.SHA256_HASH_LEN, CryptoConstants.PRIVKEY_LEN);

        MixHash(tempH);
        _state.InitializeKeyAndChainingKey(tempK, _ck);
    }

    /// <summary>
    /// Returns h. This function should only be called at the end of
    /// a handshake, i.e. after the Split() function has been called.
    /// </summary>
    public byte[] GetHandshakeHash()
    {
        ThrowIfDisposed(_disposed, nameof(Hkdf));

        return _h;
    }

    /// <summary>
    /// Sets ciphertext = EncryptWithAd(h, plaintext),
    /// calls MixHash(ciphertext), and returns ciphertext.
    /// </summary>
    public int EncryptAndHash(ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
    {
        ThrowIfDisposed(_disposed, nameof(Hkdf));

        var bytesWritten = _state.EncryptWithAd(_h, plaintext, ciphertext);
        MixHash(ciphertext[..bytesWritten]);

        return bytesWritten;
    }

    /// <summary>
    /// Sets plaintext = DecryptWithAd(h, ciphertext),
    /// calls MixHash(ciphertext), and returns plaintext.
    /// </summary>
    public int DecryptAndHash(ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
    {
        ThrowIfDisposed(_disposed, nameof(Hkdf));

        var bytesRead = _state.DecryptWithAd(_h, ciphertext, plaintext);
        MixHash(ciphertext);

        return bytesRead;
    }

    /// <summary>
    /// Returns a pair of CipherState objects for encrypting transport messages.
    /// </summary>
    public (CipherState c1, CipherState c2) Split()
    {
        ThrowIfDisposed(_disposed, nameof(Hkdf));

        Span<byte> output = stackalloc byte[2 * CryptoConstants.SHA256_HASH_LEN];
        _hkdf.ExtractAndExpand2(_ck, null, output);

        var tempK1 = output[..CryptoConstants.PRIVKEY_LEN];
        var tempK2 = output.Slice(CryptoConstants.SHA256_HASH_LEN, CryptoConstants.PRIVKEY_LEN);

        var c1 = new CipherState();
        var c2 = new CipherState();

        c1.InitializeKeyAndChainingKey(tempK1, _ck);
        c2.InitializeKeyAndChainingKey(tempK2, _ck);

        return (c1, c2);
    }

    /// <summary>
    /// Returns true if k and ck are non-empty, false otherwise.
    /// </summary>
    public bool HasKeys()
    {
        ThrowIfDisposed(_disposed, nameof(Hkdf));

        return _state.HasKeys();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _ck.Dispose();
        _state.Dispose();
        _hkdf.Dispose();
        _sha256.Dispose();
        _cryptoProvider.Dispose();

        _disposed = true;
    }
}