// Based on Noise.NET by Nemanja Mijailovic https://github.com/Metalnem/noise

using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NLightning.Bolts.BOLT8.Ciphers;

using Common;

/// <summary>
/// AEAD_CHACHA20_POLY1305 from <see href="https://tools.ietf.org/html/rfc7539">RFC 7539</see>.
/// The 96-bit nonce is formed by encoding 32 bits of zeros followed by
/// little-endian encoding of n.
/// </summary>
internal sealed class ChaCha20Poly1305
{
    /// <summary>
    /// Secret key size in bytes.
    /// </summary>
    public const int KEY_SIZE = 32;

    /// <summary>
    /// Nonce size in bytes.
    /// </summary>
    public const int NONCE_SIZE = 12;

    /// <summary>
    /// Authentication tag size in bytes.
    /// </summary>
    public const int TAG_SIZE = 16;

    /// <summary>
    /// Encrypts plaintext using the cipher key k of 32 bytes
    /// and an 8-byte unsigned integer nonce n which must be
    /// unique for the key k. Writes the result into ciphertext
    /// parameter and returns the number of bytes written. Encryption
    /// must be done with an "AEAD" encryption mode with the
    /// associated data ad and results in a ciphertext that is the
    /// same size as the plaintext plus 16 bytes for authentication data.
    /// </summary>
    public int Encrypt(ReadOnlySpan<byte> k, ulong n, ReadOnlySpan<byte> ad, ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
    {
        Debug.Assert(k.Length == KEY_SIZE);
        Debug.Assert(ciphertext.Length >= plaintext.Length + TAG_SIZE);

        Span<byte> nonce = stackalloc byte[NONCE_SIZE];
        BinaryPrimitives.WriteUInt64LittleEndian(nonce[4..], n);

        var result = Libsodium.crypto_aead_chacha20poly1305_ietf_encrypt(
            ref MemoryMarshal.GetReference(ciphertext),
            out var length,
             ref MemoryMarshal.GetReference(plaintext),
            plaintext.Length,
            ref MemoryMarshal.GetReference(ad),
            ad.Length,
            IntPtr.Zero,
            ref MemoryMarshal.GetReference(nonce),
            ref MemoryMarshal.GetReference(k)
        );

        if (result != 0)
        {
            throw new CryptographicException("Encryption failed.");
        }

        Debug.Assert(length == plaintext.Length + TAG_SIZE);
        return (int)length;
    }

    /// <summary>
    /// Decrypts ciphertext using a cipher key k of 32 bytes,
    /// an 8-byte unsigned integer nonce n, and associated data ad.
    /// Reads the result into plaintext parameter and returns the
    /// number of bytes read, unless authentication fails, in which
    /// case an error is signaled to the caller.
    /// </summary>
    public int Decrypt(ReadOnlySpan<byte> k, ulong n, ReadOnlySpan<byte> ad, ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
    {
        Debug.Assert(k.Length == KEY_SIZE);
        Debug.Assert(ciphertext.Length >= TAG_SIZE);
        Debug.Assert(plaintext.Length >= ciphertext.Length - TAG_SIZE);

        Span<byte> nonce = stackalloc byte[NONCE_SIZE];
        BinaryPrimitives.WriteUInt64LittleEndian(nonce[4..], n);

        var result = Libsodium.crypto_aead_chacha20poly1305_ietf_decrypt(
            ref MemoryMarshal.GetReference(plaintext),
            out var length,
            IntPtr.Zero,
            ref MemoryMarshal.GetReference(ciphertext),
            ciphertext.Length,
            ref MemoryMarshal.GetReference(ad),
            ad.Length,
            ref MemoryMarshal.GetReference(nonce),
            ref MemoryMarshal.GetReference(k)
        );

        if (result != 0)
        {
            throw new CryptographicException("Decryption failed.");
        }

        Debug.Assert(length == ciphertext.Length - TAG_SIZE);
        return (int)length;
    }
}