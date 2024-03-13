using System.Diagnostics;
using NLightning.Bolts.BOLT8.Noise.Constants;
using NLightning.Bolts.BOLT8.Noise.Interfaces;
using NLightning.Bolts.BOLT8.Noise.States;

namespace NLightning.Bolts.BOLT8.Noise.Primitives;

internal sealed class Transport<CipherType> : ITransport where CipherType : ICipher, new()
{
	private readonly bool _initiator;
	private readonly CipherState<CipherType> _c1;
	private readonly CipherState<CipherType> _c2;
	private bool disposed;

	public Transport(bool initiator, CipherState<CipherType> c1, CipherState<CipherType> c2)
	{
		Exceptions.ThrowIfNull(c1, nameof(c1));

		_initiator = initiator;
		_c1 = c1;
		_c2 = c2;
	}

	public bool IsOneWay
	{
		get
		{
			Exceptions.ThrowIfDisposed(disposed, nameof(Transport<CipherType>));
			return _c2 == null;
		}
	}

	public int WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer)
	{
		Exceptions.ThrowIfDisposed(disposed, nameof(Transport<CipherType>));

		if (!_initiator && IsOneWay)
		{
			throw new InvalidOperationException("Responder cannot write messages to a one-way stream.");
		}

		if (payload.Length + Aead.TAG_SIZE > Protocol.MAX_MESSAGE_LENGTH)
		{
			throw new ArgumentException($"Noise message must be less than or equal to {Protocol.MAX_MESSAGE_LENGTH} bytes in length.");
		}

		if (payload.Length + Aead.TAG_SIZE > messageBuffer.Length)
		{
			throw new ArgumentException("Message buffer does not have enough space to hold the ciphertext.");
		}

		var cipher = _initiator ? _c1 : _c2;
		Debug.Assert(cipher?.HasKey() ?? false);

		return cipher.EncryptWithAd(null, payload, messageBuffer);
	}

	public int ReadMessage(ReadOnlySpan<byte> message, Span<byte> payloadBuffer)
	{
		Exceptions.ThrowIfDisposed(disposed, nameof(Transport<CipherType>));

		if (_initiator && IsOneWay)
		{
			throw new InvalidOperationException("Initiator cannot read messages from a one-way stream.");
		}

		if (message.Length > Protocol.MAX_MESSAGE_LENGTH)
		{
			throw new ArgumentException($"Noise message must be less than or equal to {Protocol.MAX_MESSAGE_LENGTH} bytes in length.");
		}

		if (message.Length < Aead.TAG_SIZE)
		{
			throw new ArgumentException($"Noise message must be greater than or equal to {Aead.TAG_SIZE} bytes in length.");
		}

		if (message.Length - Aead.TAG_SIZE > payloadBuffer.Length)
		{
			throw new ArgumentException("Payload buffer does not have enough space to hold the plaintext.");
		}

		var cipher = _initiator ? _c2 : _c1;
		Debug.Assert(cipher?.HasKey() ?? false);

		return cipher.DecryptWithAd(null, message, payloadBuffer);
	}

	public void RekeyInitiatorToResponder()
	{
		Exceptions.ThrowIfDisposed(disposed, nameof(Transport<CipherType>));

		_c1.Rekey();
	}

	public void RekeyResponderToInitiator()
	{
		Exceptions.ThrowIfDisposed(disposed, nameof(Transport<CipherType>));

		if (IsOneWay)
		{
			throw new InvalidOperationException("Cannot rekey responder to initiator in a one-way stream.");
		}

		_c2?.Rekey();
	}

	public void Dispose()
	{
		if (!disposed)
		{
			_c1.Dispose();
			_c2?.Dispose();
			disposed = true;
		}
	}
}