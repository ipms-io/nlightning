// Based on Noise.NET by Nemanja Mijailovic https://github.com/Metalnem/noise

using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;

namespace NLightning.Infrastructure.Crypto.Ciphers;

using Domain.Crypto.Constants;
using Factories;
using Interfaces;

/// <summary>
/// AEAD_CHACHA20_POLY1305 from <see href="https://tools.ietf.org/html/rfc7539">RFC 7539</see>.
/// The 96-bit nonce is formed by encoding 32 bits of zeros followed by little-endian encoding of n.
/// </summary>
public sealed class ChaCha20Poly1305 : IDisposable
{
    private readonly ICryptoProvider _cryptoProvider;

    public ChaCha20Poly1305()
    {
        _cryptoProvider = CryptoFactory.GetCryptoProvider();
    }

    /// <summary>
    /// Encrypts plaintext using the cipher key of 32 bytes and an 8-byte unsigned integer publicNonce which must be
    /// unique for the key. Writes the result into ciphertext parameter and returns the number of bytes written.
    /// Encryption must be done with an "AEAD" encryption mode with the authenticationData and results in a ciphertext
    /// that is the same size as the plaintext plus 16 bytes for authentication data.
    /// </summary>
    public int Encrypt(ReadOnlySpan<byte> key, ulong publicNonce, ReadOnlySpan<byte> authenticationData,
                       ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
    {
        Debug.Assert(key.Length == CryptoConstants.PrivkeyLen);
        Debug.Assert(ciphertext.Length >= plaintext.Length + CryptoConstants.Chacha20Poly1305TagLen);

        Span<byte> nonce = stackalloc byte[CryptoConstants.Chacha20Poly1305NonceLen];
        BinaryPrimitives.WriteUInt64LittleEndian(nonce[4..], publicNonce);

        var result = _cryptoProvider.AeadChaCha20Poly1305IetfEncrypt(key, nonce, null, authenticationData, plaintext,
                                                                     ciphertext, out var length);

        if (result != 0)
        {
            throw new CryptographicException("Encryption failed.");
        }

        Debug.Assert(length == plaintext.Length + CryptoConstants.Chacha20Poly1305TagLen);
        return (int)length;
    }

    /// <summary>
    /// Decrypts ciphertext using a cipher key of 32 bytes, an 8-byte unsigned integer publicNonce, and
    /// authenticationData. Reads the result into plaintext parameter and returns the number of bytes read, unless
    /// authentication fails, in which case an error is signaled to the caller.
    /// </summary>
    public int Decrypt(ReadOnlySpan<byte> key, ulong publicNonce, ReadOnlySpan<byte> authenticationData,
                       ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
    {
        Debug.Assert(key.Length == CryptoConstants.PrivkeyLen);
        Debug.Assert(ciphertext.Length >= CryptoConstants.Chacha20Poly1305TagLen);
        Debug.Assert(plaintext.Length >= ciphertext.Length - CryptoConstants.Chacha20Poly1305TagLen);

        Span<byte> nonce = stackalloc byte[CryptoConstants.Chacha20Poly1305NonceLen];
        BinaryPrimitives.WriteUInt64LittleEndian(nonce[4..], publicNonce);

        var result = _cryptoProvider.AeadChaCha20Poly1305IetfDecrypt(key, nonce, null, authenticationData,
                                                                     ciphertext, plaintext, out var length);

        if (result != 0)
        {
            throw new CryptographicException("Decryption failed.");
        }

        Debug.Assert(length == ciphertext.Length - CryptoConstants.Chacha20Poly1305TagLen);
        return (int)length;
    }

    public void Dispose()
    {
        _cryptoProvider.Dispose();
    }
}