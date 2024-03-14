using System.Diagnostics;

namespace NLightning.Bolts.BOLT8.Noise.States;

using Ciphers;
using Constants;
using Hashes;
using Interfaces;
using Primitives;

/// <summary>
/// A SymmetricState object contains a CipherState plus ck (a chaining
/// key of HashLen bytes) and h (a hash output of HashLen bytes).
/// </summary>
internal sealed class SymmetricState<DhType> : IDisposable
	where DhType : IDh, new()
{
	private readonly ChaCha20Poly1305 _cipher = new();
	private readonly DhType _dh = new();
	private readonly SHA256 _hash = new();
	private readonly Hkdf _hkdf = new();
	private readonly CipherState _state = new();
	private readonly byte[] _ck;
	private readonly byte[] _h;
	private bool _disposed;

	/// <summary>
	/// Initializes a new SymmetricState with an
	/// arbitrary-length protocolName byte sequence.
	/// </summary>
	public SymmetricState(ReadOnlySpan<byte> protocolName)
	{
		int length = _hash.HashLen;

		_ck = new byte[length];
		_h = new byte[length];

		if (protocolName.Length <= length)
		{
			protocolName.CopyTo(_h);
		}
		else
		{
			_hash.AppendData(protocolName);
			_hash.GetHashAndReset(_h);
		}

		Array.Copy(_h, _ck, length);
	}

	/// <summary>
	/// Sets ck, tempK = HKDF(ck, inputKeyMaterial, 2).
	/// If HashLen is 64, then truncates tempK to 32 bytes.
	/// Calls InitializeKey(tempK).
	/// </summary>
	public void MixKey(ReadOnlySpan<byte> inputKeyMaterial)
	{
		int length = inputKeyMaterial.Length;
		Debug.Assert(length == 0 || length == Aead.KEY_SIZE || length == _dh.PrivLen);

		Span<byte> output = stackalloc byte[2 * _hash.HashLen];
		_hkdf.ExtractAndExpand2(_ck, inputKeyMaterial, output);

		output[.._hash.HashLen].CopyTo(_ck);

		var tempK = output.Slice(_hash.HashLen, Aead.KEY_SIZE);
		_state.InitializeKey(tempK);
	}

	/// <summary>
	/// Sets h = HASH(h || data).
	/// </summary>
	public void MixHash(ReadOnlySpan<byte> data)
	{
		_hash.AppendData(_h);
		_hash.AppendData(data);
		_hash.GetHashAndReset(_h);
	}

	/// <summary>
	/// Sets ck, tempH, tempK = HKDF(ck, inputKeyMaterial, 3).
	/// Calls MixHash(tempH).
	/// If HashLen is 64, then truncates tempK to 32 bytes.
	/// Calls InitializeKey(tempK).
	/// </summary>
	public void MixKeyAndHash(ReadOnlySpan<byte> inputKeyMaterial)
	{
		int length = inputKeyMaterial.Length;
		Debug.Assert(length == 0 || length == Aead.KEY_SIZE || length == _dh.PrivLen);

		Span<byte> output = stackalloc byte[3 * _hash.HashLen];
		_hkdf.ExtractAndExpand3(_ck, inputKeyMaterial, output);

		output.Slice(0, _hash.HashLen).CopyTo(_ck);

		var tempH = output.Slice(_hash.HashLen, _hash.HashLen);
		var tempK = output.Slice(2 * _hash.HashLen, Aead.KEY_SIZE);

		MixHash(tempH);
		_state.InitializeKey(tempK);
	}

	/// <summary>
	/// Returns h. This function should only be called at the end of
	/// a handshake, i.e. after the Split() function has been called.
	/// </summary>
	public byte[] GetHandshakeHash()
	{
		return _h;
	}

	/// <summary>
	/// Sets ciphertext = EncryptWithAd(h, plaintext),
	/// calls MixHash(ciphertext), and returns ciphertext.
	/// </summary>
	public int EncryptAndHash(ReadOnlySpan<byte> plaintext, Span<byte> ciphertext)
	{
		int bytesWritten = _state.EncryptWithAd(_h, plaintext, ciphertext);
		MixHash(ciphertext[..bytesWritten]);

		return bytesWritten;
	}

	/// <summary>
	/// Sets plaintext = DecryptWithAd(h, ciphertext),
	/// calls MixHash(ciphertext), and returns plaintext.
	/// </summary>
	public int DecryptAndHash(ReadOnlySpan<byte> ciphertext, Span<byte> plaintext)
	{
		var bytesRead = _state.DecryptWithAd(_h, ciphertext, plaintext);
		MixHash(ciphertext);

		return bytesRead;
	}

	/// <summary>
	/// Returns a pair of CipherState objects for encrypting transport messages.
	/// </summary>
	public (CipherState c1, CipherState c2) Split()
	{
		Span<byte> output = stackalloc byte[2 * _hash.HashLen];
		_hkdf.ExtractAndExpand2(_ck, null, output);

		var tempK1 = output[..Aead.KEY_SIZE];
		var tempK2 = output.Slice(_hash.HashLen, Aead.KEY_SIZE);

		var c1 = new CipherState();
		var c2 = new CipherState();

		c1.InitializeKeyAndChainingKey(tempK1, _ck);
		c2.InitializeKeyAndChainingKey(tempK2, _ck);

		return (c1, c2);
	}

	/// <summary>
	/// Returns true if k is non-empty, false otherwise.
	/// </summary>
	public bool HasKey()
	{
		return _state.HasKey();
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_hash.Dispose();
			_hkdf.Dispose();
			_state.Dispose();
			Utilities.ZeroMemory(_ck);
			_disposed = true;
		}
	}
}