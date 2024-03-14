using System.Diagnostics;
using NLightning.Bolts.BOLT8.Noise.Constants;
using NLightning.Bolts.BOLT8.Noise.Interfaces;

namespace NLightning.Bolts.BOLT8.Noise.States;

/// <summary>
/// A CipherState can encrypt and decrypt data based on its variables k
/// (a cipher key of 32 bytes) and n (an 8-byte unsigned integer nonce).
/// </summary>
internal sealed class CipherState<CipherType> : IDisposable where CipherType : ICipher, new()
{
	private const ulong MaxNonce = ulong.MaxValue;

	private static readonly byte[] _zeroLen = [];
	private static readonly byte[] _zeros = new byte[32];

	private readonly CipherType _cipher = new();
	private byte[]? _k;
	private ulong _n;
	private bool _disposed;

	/// <summary>
	/// Sets k = key. Sets n = 0.
	/// </summary>
	public void InitializeKey(ReadOnlySpan<byte> key)
	{
		Debug.Assert(key.Length == Aead.KEY_SIZE);

		_k ??= new byte[Aead.KEY_SIZE];
		key.CopyTo(_k);

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
		if (_n == MaxNonce)
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
		if (_n == MaxNonce)
		{
			throw new OverflowException("Nonce has reached its maximum value.");
		}

		if (_k == null)
		{
			ciphertext.CopyTo(plaintext);
			return ciphertext.Length;
		}

		int bytesRead = _cipher.Decrypt(_k, _n, ad, ciphertext, plaintext);
		++_n;

		return bytesRead;
	}

	/// <summary>
	/// Sets k = REKEY(k).
	/// </summary>
	public void Rekey()
	{
		Debug.Assert(HasKey());

		Span<byte> key = stackalloc byte[Aead.KEY_SIZE + Aead.TAG_SIZE];
		_cipher.Encrypt(_k, MaxNonce, _zeroLen, _zeros, key);

		_k ??= new byte[Aead.KEY_SIZE];
		key[Aead.KEY_SIZE..].CopyTo(_k);
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