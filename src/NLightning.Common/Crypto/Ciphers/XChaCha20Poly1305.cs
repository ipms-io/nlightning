using System.Diagnostics;
using System.Security.Cryptography;

namespace NLightning.Common.Crypto.Ciphers;

using Common.Interfaces.Crypto;
using Constants;
using Factories.Crypto;

/// <summary>
/// AEAD_XCHACHA20_POLY1305 from <see href="https://datatracker.ietf.org/doc/html/draft-irtf-cfrg-xchacha">draft-irtf-cfrg-xchacha-03</see>.
/// The 96-bit nonce is formed by encoding 32 bits of zeros followed by little-endian encoding of n.
/// </summary>
public sealed class XChaCha20Poly1305 : IDisposable
{
    private readonly ICryptoProvider _cryptoProvider;

    public XChaCha20Poly1305()
    {
        _cryptoProvider = CryptoFactory.GetCryptoProvider();
    }

    /// <summary>
    /// Encrypts plaintext using a cipher key of 32 bytes, 24-byte nonce, and optional authentication data.
    /// Writes the resulting ciphertext and authentication tag into the provided ciphertext buffer.
    /// Returns the length of the ciphertext, including the authentication tag.
    /// </summary>
    /// <param name="key">A 32-byte encryption key.</param>
    /// <param name="publicNonce">
    /// A 24-byte public nonce used for encryption, ensuring uniqueness for each encryption operation
    /// </param>
    /// <param name="authenticationData">Optional additional data to authenticate, which is not encrypted.</param>
    /// <param name="plaintext">The plaintext to be encrypted.</param>
    /// <param name="ciphertext">
    /// A buffer to store the resulting ciphertext and authentication tag.
    /// Must be large enough to hold plaintext length plus 16 bytes.
    /// </param>
    /// <returns>The total number of bytes written to the ciphertext buffer, including the authentication tag.</returns>
    /// <exception cref="CryptographicException">Thrown when the encryption process fails.</exception>
    public int Encrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce, ReadOnlySpan<byte> authenticationData,
                       ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
    {
        Debug.Assert(key.Length == CryptoConstants.PRIVKEY_LEN);
        Debug.Assert(ciphertext.Length >= plaintext.Length + CryptoConstants.XCHACHA20_POLY1305_TAG_LEN);

        var result = _cryptoProvider.AeadXChaCha20Poly1305IetfEncrypt(key, publicNonce, authenticationData, plaintext,
                                                                      ciphertext, out var length);

        if (result != 0)
        {
            throw new CryptographicException("Encryption failed.");
        }

        Debug.Assert(length == plaintext.Length + CryptoConstants.CHACHA20_POLY1305_TAG_LEN);
        return (int)length;
    }

    /// <summary>
    /// Decrypts ciphertext using a 32-byte cipher key, 24-byte nonce, and optional authentication data.
    /// Writes the resulting plaintext into the provided plaintext buffer. Returns the length of the plaintext.
    /// </summary>
    /// <param name="key">A 32-byte decryption key.</param>
    /// <param name="publicNonce">
    /// A 24-byte public nonce used for decryption. It must match the nonce used during encryption.
    /// </param>
    /// <param name="authenticationData">
    /// Optional additional data that was authenticated during encryption but not encrypted.
    /// </param>
    /// <param name="ciphertext">
    /// The ciphertext and authentication tag. The buffer should be at least the length of the plaintext plus 16 bytes.
    /// </param>
    /// <param name="plaintext">
    /// A buffer to store the resulting plaintext. It must be large enough to store the decrypted data.
    /// </param>
    /// <returns>The total number of bytes written to the plaintext buffer.</returns>
    /// <exception cref="CryptographicException">Thrown when the decryption process fails.</exception>
    public int Decrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce, ReadOnlySpan<byte> authenticationData,
                       ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
    {
        Debug.Assert(key.Length == CryptoConstants.PRIVKEY_LEN);
        Debug.Assert(ciphertext.Length >= CryptoConstants.XCHACHA20_POLY1305_TAG_LEN);
        Debug.Assert(plaintext.Length >= ciphertext.Length - CryptoConstants.XCHACHA20_POLY1305_TAG_LEN);

        var result = _cryptoProvider.AeadXChaCha20Poly1305IetfDecrypt(key, publicNonce, authenticationData, ciphertext,
                                                                      plaintext, out var length);

        if (result != 0)
        {
            throw new CryptographicException("Decryption failed.");
        }

        Debug.Assert(length == ciphertext.Length - CryptoConstants.CHACHA20_POLY1305_TAG_LEN);
        return (int)length;
    }

    public void Dispose()
    {
        _cryptoProvider.Dispose();
    }
}