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
	private IDh dh = new DhType();
	private SymmetricState<CipherType, DhType, HashType> state;
	private Protocol protocol;
	private readonly Role role;
	private Role initiator;
	private bool turnToWrite;
	private KeyPair e;
	private KeyPair s;
	private byte[] re;
	private byte[] rs;
	private bool isPsk;
	private bool isOneWay;
	private readonly Queue<MessagePattern> messagePatterns = new Queue<MessagePattern>();
	private readonly Queue<byte[]> psks = new Queue<byte[]>();
	private bool disposed;

	public HandshakeState(
		Protocol protocol,
		bool initiator,
		ReadOnlySpan<byte> prologue,
		ReadOnlySpan<byte> s,
		ReadOnlySpan<byte> rs,
		IEnumerable<byte[]> psks)
	{
		Debug.Assert(psks != null);

		if (!s.IsEmpty && s.Length != dh.DhLen)
		{
			throw new ArgumentException("Invalid local static private key.", nameof(s));
		}

		if (!rs.IsEmpty && rs.Length != dh.DhLen)
		{
			throw new ArgumentException("Invalid remote static public key.", nameof(rs));
		}

		if (s.IsEmpty && protocol.HandshakePattern.LocalStaticRequired(initiator))
		{
			throw new ArgumentException("Local static private key required, but not provided.", nameof(s));
		}

		if (!s.IsEmpty && !protocol.HandshakePattern.LocalStaticRequired(initiator))
		{
			throw new ArgumentException("Local static private key provided, but not required.", nameof(s));
		}

		if (rs.IsEmpty && protocol.HandshakePattern.RemoteStaticRequired(initiator))
		{
			throw new ArgumentException("Remote static public key required, but not provided.", nameof(rs));
		}

		if (!rs.IsEmpty && !protocol.HandshakePattern.RemoteStaticRequired(initiator))
		{
			throw new ArgumentException("Remote static public key provided, but not required.", nameof(rs));
		}

		if ((protocol.Modifiers & PatternModifiers.Fallback) != 0)
		{
			throw new ArgumentException($"Fallback modifier can only be applied by calling the {nameof(Fallback)} method.");
		}

		state = new SymmetricState<CipherType, DhType, HashType>(protocol.Name);
		state.MixHash(prologue);

		this.protocol = protocol;
		role = initiator ? Role.Alice : Role.Bob;
		this.initiator = Role.Alice;
		turnToWrite = initiator;
		this.s = s.IsEmpty ? null : dh.GenerateKeyPair(s);
		this.rs = rs.IsEmpty ? null : rs.ToArray();

		ProcessPreMessages(protocol.HandshakePattern);
		ProcessPreSharedKeys(protocol, psks);

		var pskModifiers = PatternModifiers.Psk0 | PatternModifiers.Psk1 | PatternModifiers.Psk2 | PatternModifiers.Psk3;

		isPsk = (protocol.Modifiers & pskModifiers) != 0;
		isOneWay = messagePatterns.Count == 1;
	}

	private void ProcessPreMessages(HandshakePattern handshakePattern)
	{
		foreach (var token in handshakePattern.Initiator.Tokens)
		{
			if (token == Token.S)
			{
				state.MixHash(role == Role.Alice ? s.PublicKey : rs);
			}
		}

		foreach (var token in handshakePattern.Responder.Tokens)
		{
			if (token == Token.S)
			{
				state.MixHash(role == Role.Alice ? rs : s.PublicKey);
			}
		}
	}

	private void ProcessPreSharedKeys(Protocol protocol, IEnumerable<byte[]> psks)
	{
		var patterns = protocol.HandshakePattern.Patterns;
		var modifiers = protocol.Modifiers;
		var position = 0;

		using (var enumerator = psks.GetEnumerator())
		{
			foreach (var pattern in patterns)
			{
				var modified = pattern;

				if (position == 0 && modifiers.HasFlag(PatternModifiers.Psk0))
				{
					modified = modified.PrependPsk();
					ProcessPreSharedKey(enumerator);
				}

				if (((int)modifiers & ((int)PatternModifiers.Psk1 << position)) != 0)
				{
					modified = modified.AppendPsk();
					ProcessPreSharedKey(enumerator);
				}

				messagePatterns.Enqueue(modified);
				++position;
			}

			if (enumerator.MoveNext())
			{
				throw new ArgumentException("Number of pre-shared keys was greater than the number of PSK modifiers.");
			}
		}
	}

	private void ProcessPreSharedKey(IEnumerator<byte[]> enumerator)
	{
		if (!enumerator.MoveNext())
		{
			throw new ArgumentException("Number of pre-shared keys was less than the number of PSK modifiers.");
		}

		var psk = enumerator.Current;

		if (psk.Length != Aead.KeySize)
		{
			throw new ArgumentException($"Pre-shared keys must be {Aead.KeySize} bytes in length.");
		}

		psks.Enqueue(psk.AsSpan().ToArray());
	}

	/// <summary>
	/// Overrides the DH function. It should only be used
	/// from Noise.Tests to fix the ephemeral private key.
	/// </summary>
	internal void SetDh(IDh dh)
	{
		this.dh = dh;
	}

	public ReadOnlySpan<byte> RemoteStaticPublicKey
	{
		get
		{
			ThrowIfDisposed();
			return rs;
		}
	}

	public void Fallback(Protocol protocol, ProtocolConfig config)
	{
		ThrowIfDisposed();
		Exceptions.ThrowIfNull(protocol, nameof(protocol));
		Exceptions.ThrowIfNull(config, nameof(config));

		if (protocol.HandshakePattern != HandshakePattern.XX || protocol.Modifiers != PatternModifiers.Fallback)
		{
			throw new ArgumentException("The only fallback pattern currently supported is XXfallback.");
		}

		if (config.LocalStatic == null)
		{
			throw new ArgumentException("Local static private key is required for the XXfallback pattern.");
		}

		if (initiator == Role.Bob)
		{
			throw new InvalidOperationException("Fallback cannot be applied to a Bob-initiated pattern.");
		}

		if (messagePatterns.Count + 1 != this.protocol.HandshakePattern.Patterns.Count())
		{
			throw new InvalidOperationException("Fallback can only be applied after the first handshake message.");
		}

		this.protocol = null;
		initiator = Role.Bob;
		turnToWrite = role == Role.Bob;

		s = dh.GenerateKeyPair(config.LocalStatic);
		rs = null;

		isPsk = false;
		isOneWay = false;

		while (psks.Count > 0)
		{
			var psk = psks.Dequeue();
			Utilities.ZeroMemory(psk);
		}

		state.Dispose();
		state = new SymmetricState<CipherType, DhType, HashType>(protocol.Name);
		state.MixHash(config.Prologue);

		if (role == Role.Alice)
		{
			Debug.Assert(e != null && re == null);
			state.MixHash(e.PublicKey);
		}
		else
		{
			Debug.Assert(e == null && re != null);
			state.MixHash(re);
		}

		messagePatterns.Clear();

		foreach (var pattern in protocol.HandshakePattern.Patterns.Skip(1))
		{
			messagePatterns.Enqueue(pattern);
		}
	}

	public (int, byte[], ITransport) WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer)
	{
		ThrowIfDisposed();

		if (messagePatterns.Count == 0)
		{
			throw new InvalidOperationException("Cannot call WriteMessage after the handshake has already been completed.");
		}

		var overhead = messagePatterns.Peek().Overhead(dh.DhLen, state.HasKey(), isPsk);
		var ciphertextSize = payload.Length + overhead;

		if (ciphertextSize > Protocol.MaxMessageLength)
		{
			throw new ArgumentException($"Noise message must be less than or equal to {Protocol.MaxMessageLength} bytes in length.");
		}

		if (ciphertextSize > messageBuffer.Length)
		{
			throw new ArgumentException("Message buffer does not have enough space to hold the ciphertext.");
		}

		if (!turnToWrite)
		{
			throw new InvalidOperationException("Unexpected call to WriteMessage (should be ReadMessage).");
		}

		var next = messagePatterns.Dequeue();
		var messageBufferLength = messageBuffer.Length;

		foreach (var token in next.Tokens)
		{
			switch (token)
			{
				case Token.E: messageBuffer = WriteE(messageBuffer); break;
				case Token.S: messageBuffer = WriteS(messageBuffer); break;
				case Token.EE: DhAndMixKey(e, re); break;
				case Token.ES: ProcessES(); break;
				case Token.SE: ProcessSE(); break;
				case Token.SS: DhAndMixKey(s, rs); break;
				case Token.PSK: ProcessPSK(); break;
			}
		}

		int bytesWritten = state.EncryptAndHash(payload, messageBuffer);
		int size = messageBufferLength - messageBuffer.Length + bytesWritten;

		Debug.Assert(ciphertextSize == size);

		byte[] handshakeHash = null;
		ITransport transport = null;

		if (messagePatterns.Count == 0)
		{
			(handshakeHash, transport) = Split();
		}

		turnToWrite = false;
		return (ciphertextSize, handshakeHash, transport);
	}

	private Span<byte> WriteE(Span<byte> buffer)
	{
		Debug.Assert(e == null);

		e = dh.GenerateKeyPair();
		e.PublicKey.CopyTo(buffer);
		state.MixHash(e.PublicKey);

		if (isPsk)
		{
			state.MixKey(e.PublicKey);
		}

		return buffer.Slice(e.PublicKey.Length);
	}

	private Span<byte> WriteS(Span<byte> buffer)
	{
		Debug.Assert(s != null);

		var bytesWritten = state.EncryptAndHash(s.PublicKey, buffer);
		return buffer.Slice(bytesWritten);
	}

	public (int, byte[], ITransport) ReadMessage(ReadOnlySpan<byte> message, Span<byte> payloadBuffer)
	{
		ThrowIfDisposed();

		if (messagePatterns.Count == 0)
		{
			throw new InvalidOperationException("Cannot call WriteMessage after the handshake has already been completed.");
		}

		var overhead = messagePatterns.Peek().Overhead(dh.DhLen, state.HasKey(), isPsk);
		var plaintextSize = message.Length - overhead;

		if (message.Length > Protocol.MaxMessageLength)
		{
			throw new ArgumentException($"Noise message must be less than or equal to {Protocol.MaxMessageLength} bytes in length.");
		}

		if (message.Length < overhead)
		{
			throw new ArgumentException($"Noise message must be greater than or equal to {overhead} bytes in length.");
		}

		if (plaintextSize > payloadBuffer.Length)
		{
			throw new ArgumentException("Payload buffer does not have enough space to hold the plaintext.");
		}

		if (turnToWrite)
		{
			throw new InvalidOperationException("Unexpected call to ReadMessage (should be WriteMessage).");
		}

		var next = messagePatterns.Dequeue();
		var messageLength = message.Length;

		foreach (var token in next.Tokens)
		{
			switch (token)
			{
				case Token.E: message = ReadE(message); break;
				case Token.S: message = ReadS(message); break;
				case Token.EE: DhAndMixKey(e, re); break;
				case Token.ES: ProcessES(); break;
				case Token.SE: ProcessSE(); break;
				case Token.SS: DhAndMixKey(s, rs); break;
				case Token.PSK: ProcessPSK(); break;
			}
		}

		int bytesRead = state.DecryptAndHash(message, payloadBuffer);
		Debug.Assert(bytesRead == plaintextSize);

		byte[] handshakeHash = null;
		ITransport transport = null;

		if (messagePatterns.Count == 0)
		{
			(handshakeHash, transport) = Split();
		}

		turnToWrite = true;
		return (plaintextSize, handshakeHash, transport);
	}

	private ReadOnlySpan<byte> ReadE(ReadOnlySpan<byte> buffer)
	{
		Debug.Assert(re == null);

		re = buffer.Slice(0, dh.DhLen).ToArray();
		state.MixHash(re);

		if (isPsk)
		{
			state.MixKey(re);
		}

		return buffer.Slice(re.Length);
	}

	private ReadOnlySpan<byte> ReadS(ReadOnlySpan<byte> message)
	{
		Debug.Assert(rs == null);

		var length = state.HasKey() ? dh.DhLen + Aead.TagSize : dh.DhLen;
		var temp = message.Slice(0, length);

		rs = new byte[dh.DhLen];
		state.DecryptAndHash(temp, rs);

		return message.Slice(length);
	}

	private void ProcessES()
	{
		if (role == Role.Alice)
		{
			DhAndMixKey(e, rs);
		}
		else
		{
			DhAndMixKey(s, re);
		}
	}

	private void ProcessSE()
	{
		if (role == Role.Alice)
		{
			DhAndMixKey(s, re);
		}
		else
		{
			DhAndMixKey(e, rs);
		}
	}

	private void ProcessPSK()
	{
		var psk = psks.Dequeue();
		state.MixKeyAndHash(psk);
		Utilities.ZeroMemory(psk);
	}

	private (byte[], ITransport) Split()
	{
		var (c1, c2) = state.Split();

		if (isOneWay)
		{
			c2.Dispose();
			c2 = null;
		}

		Debug.Assert(psks.Count == 0);

		var handshakeHash = state.GetHandshakeHash();
		var transport = new Transport<CipherType>(role == initiator, c1, c2);

		Clear();

		return (handshakeHash, transport);
	}

	private void DhAndMixKey(KeyPair keyPair, ReadOnlySpan<byte> publicKey)
	{
		Debug.Assert(keyPair != null);
		Debug.Assert(!publicKey.IsEmpty);

		Span<byte> sharedKey = stackalloc byte[dh.DhLen];
		dh.Dh(keyPair, publicKey, sharedKey);
		state.MixKey(sharedKey);
	}

	private void Clear()
	{
		state.Dispose();
		e?.Dispose();
		s?.Dispose();

		foreach (var psk in psks)
		{
			Utilities.ZeroMemory(psk);
		}
	}

	private void ThrowIfDisposed()
	{
		Exceptions.ThrowIfDisposed(disposed, nameof(HandshakeState<CipherType, DhType, HashType>));
	}

	public void Dispose()
	{
		if (!disposed)
		{
			Clear();
			disposed = true;
		}
	}

	private enum Role
	{
		Alice,
		Bob
	}
}