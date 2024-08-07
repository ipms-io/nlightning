using System.Diagnostics;

namespace NLightning.Bolts.BOLT8.States;

using Ciphers;
using Primitives;

/// <summary>
/// A CipherState can encrypt and decrypt data based on its variables k
/// (a cipher key of 32 bytes) and n (an 8-byte unsigned integer nonce).
/// </summary>
internal sealed class CipherState : IDisposable
{
    private const ulong MAX_NONCE = 1000;

    private readonly ChaCha20Poly1305 _cipher = new();
    private readonly Hkdf _hkdf = new();
    private byte[]? _ck;
    private byte[]? _k;
    private ulong _n;
    private bool _disposed;

    /// <summary>
    /// Sets k = key. Sets n = 0.
    /// </summary>
    public void InitializeKey(ReadOnlySpan<byte> key)
    {
        Debug.Assert(key.Length == ChaCha20Poly1305.KEY_SIZE);

        _k ??= new byte[ChaCha20Poly1305.KEY_SIZE];
        key.CopyTo(_k);

        _n = 0;
    }

    /// <summary>
    /// Sets _k = key. Sets _ck = ck. Sets _n = 0.
    /// </summary>
    public void InitializeKeyAndChainingKey(ReadOnlySpan<byte> key, ReadOnlySpan<byte> chainingKey)
    {
        Debug.Assert(key.Length == ChaCha20Poly1305.KEY_SIZE);
        Debug.Assert(chainingKey.Length == ChaCha20Poly1305.KEY_SIZE);

        _k ??= new byte[ChaCha20Poly1305.KEY_SIZE];
        key.CopyTo(_k);

        _ck ??= new byte[ChaCha20Poly1305.KEY_SIZE];
        chainingKey.CopyTo(_ck);

        _n = 0;
    }

    /// <summary>
    /// Returns true if k is non-empty, false otherwise.
    /// </summary>
    public bool HasKey()
    {
        return _k != null;
    }

    /// <summary>
    /// Sets n = nonce. This function is used for handling out-of-order transport messages.
    /// </summary>
    public void SetNonce(ulong nonce)
    {
        _n = nonce;
    }

    /// <summary>
    /// If k is non-empty returns ENCRYPT(k, n++, ad, plaintext).
    /// Otherwise copies the plaintext to the ciphertext parameter
    /// and returns the length of the plaintext.
    /// </summary>
    public int EncryptWithAd(ReadOnlySpan<byte> ad, ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
    {
        if (_n == MAX_NONCE)
        {
            throw new OverflowException("Nonce has reached its maximum value.");
        }

        if (_k == null)
        {
            plaintext.CopyTo(ciphertext);
            return plaintext.Length;
        }

        return _cipher.Encrypt(_k, _n++, ad, plaintext, ciphertext);
    }

    /// <summary>
    /// If k is non-empty returns DECRYPT(k, n++, ad, ciphertext).
    /// Otherwise copies the ciphertext to the plaintext parameter and returns
    /// the length of the ciphertext. If an authentication failure occurs
    /// then n is not incremented and an error is signaled to the caller.
    /// </summary>
    public int DecryptWithAd(ReadOnlySpan<byte> ad, ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
    {
        // If nonce reaches its maximum value, rekey
        if (_n == MAX_NONCE)
        {
            throw new OverflowException("Nonce has reached its maximum value.");
        }

        if (_k == null)
        {
            ciphertext.CopyTo(plaintext);
            return ciphertext.Length;
        }

        var bytesRead = _cipher.Decrypt(_k, _n, ad, ciphertext, plaintext);
        ++_n;

        return bytesRead;
    }

    /// <summary>
    /// Returns ENCRYPT(k, n, null, plaintext).
    /// </summary>
    /// <param name="plaintext">Bytes to be encrypted</param>
    /// <param name="ciphertext">Encrypted bytes</param>
    /// <returns>Number of bytes written to ciphertext</returns>
    public int Encrypt(ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
    {
        if (_n == MAX_NONCE)
        {
            Rekey();
        }

        return _cipher.Encrypt(_k, _n++, null, plaintext, ciphertext);
    }

    /// <summary>
    /// Returns DECRYPT(k, n, null, ciphertext).
    /// </summary>
    /// <param name="ciphertext">Bytes to be decrypted</param>
    /// <param name="plaintext">Decrypted bytes</param>
    /// <returns>Number of bytes written to plaintext</returns>
    public int Decrypt(ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
    {
        if (_n == MAX_NONCE)
        {
            Rekey();
        }
        return _cipher.Decrypt(_k, _n++, null, ciphertext, plaintext);
    }

    /// <summary>
    /// Sets k = REKEY(k).
    /// </summary>
    public void Rekey()
    {
        Debug.Assert(HasKey());

        _n = 0;

        Span<byte> key = stackalloc byte[ChaCha20Poly1305.KEY_SIZE * 2];
        _hkdf.ExtractAndExpand2(_ck, _k, key);

        _ck ??= new byte[ChaCha20Poly1305.KEY_SIZE];
        key[..ChaCha20Poly1305.KEY_SIZE].CopyTo(_ck);

        _k ??= new byte[ChaCha20Poly1305.KEY_SIZE];
        key[ChaCha20Poly1305.KEY_SIZE..].CopyTo(_k);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Utilities.ZeroMemory(_k);
            _disposed = true;
        }
    }
}