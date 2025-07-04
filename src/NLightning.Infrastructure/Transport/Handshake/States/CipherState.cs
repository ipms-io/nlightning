using System.Diagnostics;

namespace NLightning.Infrastructure.Transport.Handshake.States;

using Crypto.Ciphers;
using Crypto.Functions;
using Crypto.Primitives;
using Domain.Crypto.Constants;
using Domain.Utils;

/// <summary>
/// A CipherState can encrypt and decrypt data based on its variables k
/// (a cipher key of 32 bytes) and n (an 8-byte unsigned integer nonce).
/// </summary>
internal sealed class CipherState : IDisposable
{
    private const ulong MaxNonce = 1000;

    private readonly ChaCha20Poly1305 _cipher = new();
    private readonly Hkdf _hkdf = new();

    private SecureMemory? _ck;
    private SecureMemory? _k;
    private ulong _n;
    private bool _disposed;

    /// <summary>
    /// Sets _k = key. Sets _ck = ck. Sets _n = 0.
    /// </summary>
    public void InitializeKeyAndChainingKey(ReadOnlySpan<byte> key, ReadOnlySpan<byte> chainingKey)
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(CipherState));

        Debug.Assert(key.Length == CryptoConstants.PrivkeyLen);
        Debug.Assert(chainingKey.Length == CryptoConstants.PrivkeyLen);

        _k ??= new SecureMemory(CryptoConstants.PrivkeyLen);
        key.CopyTo(_k);

        _ck ??= new SecureMemory(CryptoConstants.PrivkeyLen);
        chainingKey.CopyTo(_ck);

        _n = 0;
    }

    /// <summary>
    /// Returns true if k and ck are non-empty, false otherwise.
    /// </summary>
    public bool HasKeys()
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(CipherState));

        return _k is not null && _ck is not null;
    }

    /// <summary>
    /// Sets n = nonce. This function is used for handling out-of-order transport messages.
    /// </summary>
    public void SetNonce(ulong nonce)
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(CipherState));

        _n = nonce;
    }

    /// <summary>
    /// If k is non-empty returns ENCRYPT(k, n++, ad, plaintext).
    /// Otherwise, copies the plaintext to the ciphertext parameter
    /// and returns the length of the plaintext.
    /// </summary>
    public int EncryptWithAd(ReadOnlySpan<byte> ad, ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(CipherState));

        if (_n == MaxNonce)
        {
            throw new OverflowException("Nonce has reached its maximum value.");
        }

        if (_k != null)
        {
            return _cipher.Encrypt(_k, _n++, ad, plaintext, ciphertext);
        }

        plaintext.CopyTo(ciphertext);
        return plaintext.Length;
    }

    /// <summary>
    /// If k is non-empty returns DECRYPT(k, n++, ad, ciphertext).
    /// Otherwise, copies the ciphertext to the plaintext parameter and returns
    /// the length of the ciphertext. If an authentication failure occurs
    /// then n is not incremented and an error is signaled to the caller.
    /// </summary>
    public int DecryptWithAd(ReadOnlySpan<byte> ad, ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(CipherState));

        // If nonce reaches its maximum value, rekey
        if (_n == MaxNonce)
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
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(CipherState));

        if (_n == MaxNonce)
        {
            Rekey();
        }

        return _cipher.Encrypt(_k!, _n++, null, plaintext, ciphertext);
    }

    /// <summary>
    /// Returns DECRYPT(k, n, null, ciphertext).
    /// </summary>
    /// <param name="ciphertext">Bytes to be decrypted</param>
    /// <param name="plaintext">Decrypted bytes</param>
    /// <returns>Number of bytes written to plaintext</returns>
    public int Decrypt(ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(CipherState));

        if (_n == MaxNonce)
        {
            Rekey();
        }
        return _cipher.Decrypt(_k!, _n++, null, ciphertext, plaintext);
    }

    /// <summary>
    /// Sets k = REKEY(k).
    /// </summary>
    public void Rekey()
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(CipherState));

        if (!HasKeys())
        {
            throw new NullReferenceException("Keys are missing");
        }

        _n = 0;

        Span<byte> key = stackalloc byte[CryptoConstants.PrivkeyLen * 2];
        _hkdf.ExtractAndExpand2(_ck!, _k!, key);

        _ck ??= new SecureMemory(CryptoConstants.PrivkeyLen);
        key[..CryptoConstants.PrivkeyLen].CopyTo(_ck);

        _k ??= new SecureMemory(CryptoConstants.PrivkeyLen);
        key[CryptoConstants.PrivkeyLen..].CopyTo(_k);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _ck?.Dispose();
        _k?.Dispose();
        _hkdf.Dispose();
        _cipher.Dispose();

        _disposed = true;
    }
}