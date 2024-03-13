using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NLightning.Bolts.BOLT8.Noise.Ciphers;

using Constants;
using Interfaces;

/// <summary>
/// AEAD_CHACHA20_POLY1305 from <see href="https://tools.ietf.org/html/rfc7539">RFC 7539</see>.
/// The 96-bit nonce is formed by encoding 32 bits
/// of zeros followed by little-endian encoding of n.
/// </summary>
internal sealed class ChaCha20Poly1305 : ICipher
{
	/// <inheritdoc/>
	public int Encrypt(ReadOnlySpan<byte> k, ulong n, ReadOnlySpan<byte> ad, ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
	{
		Debug.Assert(k.Length == Aead.KEY_SIZE);
		Debug.Assert(ciphertext.Length >= plaintext.Length + Aead.TAG_SIZE);

		Span<byte> nonce = stackalloc byte[Aead.NONCE_SIZE];
		BinaryPrimitives.WriteUInt64LittleEndian(nonce[4..], n);

		int result = Libsodium.crypto_aead_chacha20poly1305_ietf_encrypt(
			ref MemoryMarshal.GetReference(ciphertext),
			out long length,
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

		Debug.Assert(length == plaintext.Length + Aead.TAG_SIZE);
		return (int)length;
	}

	/// <inheritdoc/>
	public int Decrypt(ReadOnlySpan<byte> k, ulong n, ReadOnlySpan<byte> ad, ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
	{
		Debug.Assert(k.Length == Aead.KEY_SIZE);
		Debug.Assert(ciphertext.Length >= Aead.TAG_SIZE);
		Debug.Assert(plaintext.Length >= ciphertext.Length - Aead.TAG_SIZE);

		Span<byte> nonce = stackalloc byte[Aead.NONCE_SIZE];
		BinaryPrimitives.WriteUInt64LittleEndian(nonce[4..], n);

		int result = Libsodium.crypto_aead_chacha20poly1305_ietf_decrypt(
			ref MemoryMarshal.GetReference(plaintext),
			out long length,
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

		Debug.Assert(length == ciphertext.Length - Aead.TAG_SIZE);
		return (int)length;
	}
}