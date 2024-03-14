using System.Diagnostics;
using NLightning.Bolts.BOLT8.Noise.Constants;
using NLightning.Bolts.BOLT8.Noise.Interfaces;
using NLightning.Bolts.BOLT8.Noise.States;

namespace NLightning.Bolts.BOLT8.Noise.Primitives;

/// <inheritdoc/>
internal sealed class Transport<CipherType> : ITransport where CipherType : ICipher, new()
{
	private const int MAX_NONCE = 1000;
	private const int LC_SIZE = 18;

	private readonly bool _initiator;
	private readonly CipherState<CipherType> _sendingKey;
	private readonly CipherState<CipherType> _receivingKey;

	private int _writeNonce = 0;
	private int _readNonce = 0;
	private bool disposed;

	public Transport(bool initiator, CipherState<CipherType> c1, CipherState<CipherType> c2)
	{
		Exceptions.ThrowIfNull(c1, nameof(c1));

		_initiator = initiator;
		_sendingKey = c1;
		_receivingKey = c2;
	}

	/// <inheritdoc/>
	public int WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer)
	{
		// Serialize length into 2 bytes encoded as a big-endian integer
		var l = BitConverter.GetBytes((ushort)payload.Length).Reverse().ToArray();
		// Encrypt the payload length into the message buffer
		var lcLen = WriteMessagePart(l, messageBuffer);

		// Encrypt the payload into the message buffer
		var mLen = WriteMessagePart(payload, messageBuffer[lcLen..]);

		return lcLen + mLen;
	}

	/// <inheritdoc/>
	public int ReadMessageLength(ReadOnlySpan<byte> lc)
	{
		Exceptions.ThrowIfDisposed(disposed, nameof(Transport<CipherType>));

		if (lc.Length != LC_SIZE)
		{
			throw new ArgumentException($"Lightning Message Header must be {LC_SIZE} bytes in length.");
		}

		// Decrypt the payload length from the message buffer
		var l = new byte[2];
		var lcLen = ReadMessagePart(lc, l);
		return BitConverter.ToUInt16(l.Reverse().ToArray(), 0) + Aead.TAG_SIZE;
	}

	/// <inheritdoc/>
	public int ReadMessagePayload(ReadOnlySpan<byte> message, Span<byte> payloadBuffer)
	{
		// Decrypt the payload from the message buffer
		return ReadMessagePart(message, payloadBuffer);
	}

	/// <summary>
	/// Encrypts the <paramref name="payload"/> and writes the result into <paramref name="messageBuffer"/>.
	/// </summary>
	/// <param name="payload">The payload to encrypt.</param>
	/// <param name="messageBuffer">The buffer for the encrypted message.</param>
	/// <returns>The ciphertext size in bytes.</returns>
	/// <exception cref="ObjectDisposedException">
	/// Thrown if the current instance has already been disposed.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the responder has attempted to write a message to a one-way stream.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// Thrown if the encrypted payload was greater than <see cref="Protocol.MaxMessageLength"/>
	/// bytes in length, or if the output buffer did not have enough space to hold the ciphertext.
	/// </exception>
	private int WriteMessagePart(ReadOnlySpan<byte> payload, Span<byte> messageBuffer)
	{
		Exceptions.ThrowIfDisposed(disposed, nameof(Transport<CipherType>));

		if (payload.Length + Aead.TAG_SIZE > Protocol.MAX_MESSAGE_LENGTH)
		{
			throw new ArgumentException($"Noise message must be less than or equal to {Protocol.MAX_MESSAGE_LENGTH} bytes in length.");
		}

		if (payload.Length + Aead.TAG_SIZE > messageBuffer.Length)
		{
			throw new ArgumentException("Message buffer does not have enough space to hold the ciphertext.");
		}

		if (_writeNonce > MAX_NONCE)
		{
			RekeyInitiatorToResponder();
		}

		var cipher = _initiator ? _sendingKey : _receivingKey;
		Debug.Assert(cipher?.HasKey() ?? false);

		_writeNonce++;
		return cipher.EncryptWithAd(null, payload, messageBuffer);
	}

	/// <summary>
	/// Decrypts the <paramref name="message"/> and writes the result into <paramref name="payloadBuffer"/>.
	/// </summary>
	/// <param name="message">The message to decrypt.</param>
	/// <param name="payloadBuffer">The buffer for the decrypted payload.</param>
	/// <returns>The plaintext size in bytes.</returns>
	/// <exception cref="ObjectDisposedException">
	/// Thrown if the current instance has already been disposed.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the initiator has attempted to read a message from a one-way stream.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// Thrown if the message was greater than <see cref="Protocol.MaxMessageLength"/>
	/// bytes in length, or if the output buffer did not have enough space to hold the plaintext.
	/// </exception>
	/// <exception cref="System.Security.Cryptography.CryptographicException">
	/// Thrown if the decryption of the message has failed.
	/// </exception>
	private int ReadMessagePart(ReadOnlySpan<byte> message, Span<byte> payloadBuffer)
	{
		Exceptions.ThrowIfDisposed(disposed, nameof(Transport<CipherType>));

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

		if (_readNonce > MAX_NONCE)
		{
			RekeyResponderToInitiator();
		}

		var cipher = _initiator ? _receivingKey : _sendingKey;
		Debug.Assert(cipher?.HasKey() ?? false);

		_readNonce++;
		return cipher.DecryptWithAd(null, message, payloadBuffer);
	}

	/// <summary>
	/// Updates the symmetric key used to encrypt transport messages from
	/// initiator to responder using a one-way function, so that a compromise
	/// of keys will not decrypt older messages.
	/// </summary>
	/// <exception cref="ObjectDisposedException">
	/// Thrown if the current instance has already been disposed.
	/// </exception>
	private void RekeyInitiatorToResponder()
	{
		Exceptions.ThrowIfDisposed(disposed, nameof(Transport<CipherType>));

		_sendingKey.Rekey();
	}

	/// <summary>
	/// Updates the symmetric key used to encrypt transport messages from
	/// responder to initiator using a one-way function, so that a compromise
	/// of keys will not decrypt older messages.
	/// </summary>
	/// <exception cref="ObjectDisposedException">
	/// Thrown if the current instance has already been disposed.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the current instance is a one-way stream.
	/// </exception>
	private void RekeyResponderToInitiator()
	{
		Exceptions.ThrowIfDisposed(disposed, nameof(Transport<CipherType>));

		_receivingKey.Rekey();
	}

	public void Dispose()
	{
		if (!disposed)
		{
			_sendingKey.Dispose();
			_receivingKey.Dispose();
			disposed = true;
		}
	}
}