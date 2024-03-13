using System.Diagnostics;
using NLightning.Bolts.BOLT8.Noise.Constants;
using NLightning.Bolts.BOLT8.Noise.Enums;
using NLightning.Bolts.BOLT8.Noise.Interfaces;
using NLightning.Bolts.BOLT8.Noise.MessagePatterns;
using NLightning.Bolts.BOLT8.Noise.Primitives;

namespace NLightning.Bolts.BOLT8.Noise.States;

internal sealed class HandshakeState<CipherType, DhType, HashType> : IHandshakeState
	where CipherType : ICipher, new()
	where DhType : IDh, new()
	where HashType : IHash, new()
{
	private const byte HANDSHAKE_VERSION = 0x00;

	private readonly SymmetricState<CipherType, DhType, HashType> _state;
	private readonly Protocol _protocol;
	private readonly Role _role;
	private readonly Role _initiator;
	private readonly KeyPair _s;
	private readonly Queue<MessagePattern> _messagePatterns = new();

	private IDh _dh = new DhType();
	private KeyPair? _e;
	private byte[]? _re;
	private byte[] _rs;
	private bool _turnToWrite;
	private bool _disposed;

	/// <summary>
	/// Creates a new HandshakeState instance.
	/// </summary>
	/// <param name="protocol">A concrete Noise protocol</param>
	/// <param name="initiator">If we are the initiator</param>
	/// <param name="prologue">A prologue (lightning)</param>
	/// <param name="s">Local Static Private Key</param>
	/// <param name="rs">Remote Static Public Key</param>
	/// <exception cref="ArgumentException"></exception>
	public HandshakeState(Protocol protocol, bool initiator, ReadOnlySpan<byte> prologue, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs)
	{
		if (s.IsEmpty)
		{
			throw new ArgumentException("Local static private key required, but not provided.", nameof(s));
		}

		if (s.Length != _dh.PrivLen)
		{
			throw new ArgumentException("Invalid local static private key.", nameof(s));
		}

		if (rs.IsEmpty)
		{
			throw new ArgumentException("Remote static public key required, but not provided.", nameof(rs));
		}

		if (rs.Length != _dh.PubLen)
		{
			throw new ArgumentException("Invalid remote static public key.", nameof(rs));
		}

		_state = new SymmetricState<CipherType, DhType, HashType>(protocol.Name);
		_state.MixHash(prologue);

		_protocol = protocol;
		_role = initiator ? Role.Alice : Role.Bob;
		_initiator = Role.Alice;
		_turnToWrite = initiator;
		_s = _dh.GenerateKeyPair(s);
		_rs = rs.ToArray();

		ProcessPreMessages();
		EnqueueMessages();
	}

	private void ProcessPreMessages()
	{
		foreach (var token in _protocol.HandshakePattern.Initiator.Tokens)
		{
			if (token == Token.S)
			{
				_state.MixHash(_role == Role.Alice ? _s.PublicKeyBytes : _rs);
			}
		}

		foreach (var token in _protocol.HandshakePattern.Responder.Tokens)
		{
			if (token == Token.S)
			{
				_state.MixHash(_role == Role.Alice ? _rs : _s.PublicKeyBytes);
			}
		}
	}

	private void EnqueueMessages()
	{
		foreach (var pattern in _protocol.HandshakePattern.Patterns)
		{
			_messagePatterns.Enqueue(pattern);
		}
	}

	/// <summary>
	/// Overrides the DH function. It should only be used
	/// from Noise.Tests to fix the ephemeral private key.
	/// </summary>
	internal void SetDh(IDh dh)
	{
		_dh = dh;
	}

	public ReadOnlySpan<byte> RemoteStaticPublicKey
	{
		get
		{
			ThrowIfDisposed();
			return _rs;
		}
	}

	public (int, byte[]?, ITransport?) WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer)
	{
		ThrowIfDisposed();

		if (_messagePatterns.Count == 0)
		{
			throw new InvalidOperationException("Cannot call WriteMessage after the handshake has already been completed.");
		}

		var overhead = _messagePatterns.Peek().Overhead(_dh.PubLen, _state.HasKey());
		var ciphertextSize = payload.Length + overhead;

		if (ciphertextSize > Protocol.MAX_MESSAGE_LENGTH)
		{
			throw new ArgumentException($"Noise message must be less than or equal to {Protocol.MAX_MESSAGE_LENGTH} bytes in length.");
		}

		if (ciphertextSize > messageBuffer.Length)
		{
			throw new ArgumentException("Message buffer does not have enough space to hold the ciphertext.");
		}

		if (!_turnToWrite)
		{
			throw new InvalidOperationException("Unexpected call to WriteMessage (should be ReadMessage).");
		}

		var next = _messagePatterns.Dequeue();
		var messageBufferLength = messageBuffer.Length;

		// write version to message buffer
		messageBuffer[0] = HANDSHAKE_VERSION;

		foreach (var token in next.Tokens)
		{
			switch (token)
			{
				case Token.E: messageBuffer = WriteE(messageBuffer); break;
				case Token.S: messageBuffer = WriteS(messageBuffer); break;
				case Token.EE: DhAndMixKey(_e, _re); break;
				case Token.ES: ProcessES(); break;
				case Token.SE: ProcessSE(); break;
				case Token.SS: DhAndMixKey(_s, _rs); break;
			}
		}

		int bytesWritten = _state.EncryptAndHash(payload, messageBuffer);
		int size = messageBufferLength - messageBuffer.Length + bytesWritten;

		Debug.Assert(ciphertextSize == size);

		byte[]? handshakeHash = null;
		ITransport? transport = null;

		if (_messagePatterns.Count == 0)
		{
			(handshakeHash, transport) = Split();
		}

		_turnToWrite = false;
		return (ciphertextSize, handshakeHash, transport);
	}

	private Span<byte> WriteE(Span<byte> buffer)
	{
		Debug.Assert(_e == null);

		_e = _dh.GenerateKeyPair();
		// Start from position 1, since we need our version there
		_e.PublicKeyBytes.CopyTo(buffer[1..]);
		_state.MixHash(_e.PublicKeyBytes);

		// Don't forget to add our version length to the resulting Span
		return buffer[(_e.PublicKeyBytes.Length + 1)..];
	}

	private Span<byte> WriteS(Span<byte> buffer)
	{
		Debug.Assert(_s != null);

		// Start from position 1, since we need our version there
		var bytesWritten = _state.EncryptAndHash(_s.PublicKeyBytes, buffer[1..]);

		// Don't forget to add our version length to the resulting Span
		return buffer[(bytesWritten + 1)..];
	}

	public (int, byte[]?, ITransport?) ReadMessage(ReadOnlySpan<byte> message, Span<byte> payloadBuffer)
	{
		ThrowIfDisposed();

		if (_messagePatterns.Count == 0)
		{
			throw new InvalidOperationException("Cannot call WriteMessage after the handshake has already been completed.");
		}

		var overhead = _messagePatterns.Peek().Overhead(_dh.PubLen, _state.HasKey());
		var plaintextSize = message.Length - overhead;

		if (message.Length > Protocol.MAX_MESSAGE_LENGTH)
		{
			throw new ArgumentException($"Noise message must be less than or equal to {Protocol.MAX_MESSAGE_LENGTH} bytes in length.");
		}

		if (message.Length != overhead)
		{
			throw new ArgumentException($"Noise message must be equal to {overhead} bytes in length.");
		}

		if (plaintextSize > payloadBuffer.Length)
		{
			throw new ArgumentException("Payload buffer does not have enough space to hold the plaintext.");
		}

		if (_turnToWrite)
		{
			throw new InvalidOperationException("Unexpected call to ReadMessage (should be WriteMessage).");
		}

		var next = _messagePatterns.Dequeue();
		var messageLength = message.Length;

		foreach (var token in next.Tokens)
		{
			switch (token)
			{
				case Token.E: message = ReadE(message); break;
				case Token.S: message = ReadS(message); break;
				case Token.EE: DhAndMixKey(_e, _re); break;
				case Token.ES: ProcessES(); break;
				case Token.SE: ProcessSE(); break;
				case Token.SS: DhAndMixKey(_s, _rs); break;
			}
		}

		int bytesRead = _state.DecryptAndHash(message, payloadBuffer);
		Debug.Assert(bytesRead == plaintextSize);

		byte[]? handshakeHash = null;
		ITransport? transport = null;

		if (_messagePatterns.Count == 0)
		{
			(handshakeHash, transport) = Split();
		}

		_turnToWrite = true;
		return (plaintextSize, handshakeHash, transport);
	}

	private ReadOnlySpan<byte> ReadE(ReadOnlySpan<byte> buffer)
	{
		Debug.Assert(_re == null);

		// Check version
		if (buffer[0] != HANDSHAKE_VERSION)
		{
			throw new InvalidOperationException("Invalid handshake version.");
		}
		buffer = buffer[1..];

		// Skip the byte from the version and get all bytes from pubkey
		_re = buffer[.._dh.PubLen].ToArray();
		_state.MixHash(_re);

		return buffer[_re.Length..];
	}

	private ReadOnlySpan<byte> ReadS(ReadOnlySpan<byte> message)
	{
		// Debug.Assert(_rs == null);

		// Check version
		if (message[0] != HANDSHAKE_VERSION)
		{
			throw new InvalidOperationException("Invalid handshake version.");
		}
		message = message[1..];

		var length = _state.HasKey() ? _dh.PubLen + Aead.TAG_SIZE : _dh.PubLen;
		var temp = message[..length];

		_rs = new byte[_dh.PubLen];
		_state.DecryptAndHash(temp, _rs);

		return message[length..];
	}

	private void ProcessES()
	{
		if (_role == Role.Alice)
		{
			DhAndMixKey(_e, _rs);
		}
		else
		{
			DhAndMixKey(_s, _re);
		}
	}

	private void ProcessSE()
	{
		if (_role == Role.Alice)
		{
			DhAndMixKey(_s, _re);
		}
		else
		{
			DhAndMixKey(_e, _rs);
		}
	}

	private (byte[], ITransport) Split()
	{
		var (c1, c2) = _state.Split();

		var handshakeHash = _state.GetHandshakeHash();
		var transport = new Transport<CipherType>(_role == _initiator, c1, c2);

		Clear();

		return (handshakeHash, transport);
	}

	private void DhAndMixKey(KeyPair? keyPair, ReadOnlySpan<byte> publicKey)
	{
		Debug.Assert(keyPair != null);
		Debug.Assert(!publicKey.IsEmpty);

		Span<byte> sharedKey = stackalloc byte[_dh.PrivLen];
		_dh.Dh(keyPair.PrivateKey, publicKey, sharedKey);
		_state.MixKey(sharedKey);
	}

	private void Clear()
	{
		_state.Dispose();
		_e?.Dispose();
		_s?.Dispose();
	}

	private void ThrowIfDisposed()
	{
		Exceptions.ThrowIfDisposed(_disposed, nameof(HandshakeState<CipherType, DhType, HashType>));
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			Clear();
			_disposed = true;
		}
	}

	private enum Role
	{
		Alice,
		Bob
	}
}