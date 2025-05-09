using System.Diagnostics;
using NLightning.Infrastructure.Transport.Handshake.Enums;
using NLightning.Infrastructure.Transport.Handshake.MessagePatterns;
using NLightning.Infrastructure.Transport.Interfaces;

namespace NLightning.Infrastructure.Transport.Handshake.States;

using Crypto.Constants;
using Crypto.Functions;
using Crypto.Interfaces;
using Crypto.Primitives;
using Protocol.Constants;

/// <inheritdoc/>
/// <remarks> See <see href="https://github.com/lightning/bolts/blob/master/08-transport.md">Lightning Bolt8</see> for specific implementation information.</remarks>
internal sealed class HandshakeState : IHandshakeState
{
    private const byte HANDSHAKE_VERSION = 0x00;
    private static readonly HandshakePattern s_handshakePattern = HandshakePattern.XK;

    private readonly SymmetricState _state;
    private readonly Role _role;
    private readonly Role _initiator;
    private readonly KeyPair _s;
    private readonly Queue<MessagePattern> _messagePatterns = new();

    private readonly IEcdh _dh;
    private KeyPair? _e;
    private byte[]? _re;
    private byte[] _rs;
    private bool _turnToWrite;
    private bool _disposed;

    public NBitcoin.PubKey RemoteStaticPublicKey => new(_rs);

    /// <summary>
    /// Creates a new HandshakeState instance.
    /// </summary>
    /// <param name="initiator">If we are the initiator</param>
    /// <param name="s">Local Static Private Key</param>
    /// <param name="rs">Remote Static Public Key</param>
    /// <param name="ecdh">A specific DH Function, or null to use the <see cref="Ecdh">Protocol Default</see></param>
    /// <exception cref="ArgumentException"></exception>
    public HandshakeState(bool initiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, IEcdh? ecdh = null)
    {
        _dh = ecdh ?? new Ecdh();

        if (s.IsEmpty)
        {
            throw new ArgumentException("Local static private key required, but not provided.", nameof(s));
        }

        if (s.Length != CryptoConstants.PRIVKEY_LEN)
        {
            throw new ArgumentException("Invalid local static private key.", nameof(s));
        }

        if (rs.IsEmpty)
        {
            throw new ArgumentException("Remote static public key required, but not provided.", nameof(rs));
        }

        if (rs.Length != CryptoConstants.PUBKEY_LEN)
        {
            throw new ArgumentException("Invalid remote static public key.", nameof(rs));
        }

        _state = new SymmetricState(ProtocolConstants.NAME);
        _state.MixHash(ProtocolConstants.PROLOGUE);

        _role = initiator ? Role.ALICE : Role.BOB;
        _initiator = Role.ALICE;
        _turnToWrite = initiator;
        _s = _dh.GenerateKeyPair(s);
        _rs = rs.ToArray();

        ProcessPreMessages();
        EnqueueMessages();
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">Thrown if the current instance has already been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the call to <see cref="ReadMessage"/> was expected or the handshake has already been completed.</exception>
    /// <exception cref="ArgumentException">Thrown if the output was greater than <see cref="ProtocolConstants.MAX_MESSAGE_LENGTH"/> bytes in length, or if the output buffer did not have enough space to hold the ciphertext.</exception>
    public (int, byte[]?, Encryption.Transport?) WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer)
    {
        ThrowIfDisposed(_disposed, nameof(HandshakeState));

        if (_messagePatterns.Count == 0)
        {
            throw new InvalidOperationException("Cannot call WriteMessage after the handshake has already been completed.");
        }

        var overhead = _messagePatterns.Peek().Overhead(CryptoConstants.PUBKEY_LEN, _state.HasKeys());
        var ciphertextSize = payload.Length + overhead;

        if (ciphertextSize > ProtocolConstants.MAX_MESSAGE_LENGTH)
        {
            throw new ArgumentException($"Noise message must be less than or equal to {ProtocolConstants.MAX_MESSAGE_LENGTH} bytes in length.");
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
                case Token.ES: ProcessEs(); break;
                case Token.SE: ProcessSe(); break;
                case Token.SS: DhAndMixKey(_s, _rs); break;
            }
        }

        var bytesWritten = _state.EncryptAndHash(payload, messageBuffer);
        var size = messageBufferLength - messageBuffer.Length + bytesWritten;

        Debug.Assert(ciphertextSize == size);

        byte[]? handshakeHash = null;
        Encryption.Transport? transport = null;

        if (_messagePatterns.Count == 0)
        {
            (handshakeHash, transport) = Split();
        }

        _turnToWrite = false;
        return (ciphertextSize, handshakeHash, transport);
    }

    private void ThrowIfDisposed(bool disposed, string handshakeStateName)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">Thrown if the current instance has already been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the call to <see cref="WriteMessage"/> was expected or the handshake has already been completed.</exception>
    /// <exception cref="ArgumentException">Thrown if the message was greater than <see cref="ProtocolConstants.MAX_MESSAGE_LENGTH"/> bytes in length, or if the output buffer did not have enough space to hold the plaintext.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown if the decryption of the message has failed.</exception>
    public (int, byte[]?, Encryption.Transport?) ReadMessage(ReadOnlySpan<byte> message, Span<byte> payloadBuffer)
    {
        ThrowIfDisposed(_disposed, nameof(HandshakeState));

        if (_messagePatterns.Count == 0)
        {
            throw new InvalidOperationException("Cannot call WriteMessage after the handshake has already been completed.");
        }

        var overhead = _messagePatterns.Peek().Overhead(CryptoConstants.PUBKEY_LEN, _state.HasKeys());
        var plaintextSize = message.Length - overhead;

        if (message.Length > ProtocolConstants.MAX_MESSAGE_LENGTH)
        {
            throw new ArgumentException($"Noise message must be less than or equal to {ProtocolConstants.MAX_MESSAGE_LENGTH} bytes in length.");
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
        foreach (var token in next.Tokens)
        {
            switch (token)
            {
                case Token.E: message = ReadE(message); break;
                case Token.S: message = ReadS(message); break;
                case Token.EE: DhAndMixKey(_e, _re); break;
                case Token.ES: ProcessEs(); break;
                case Token.SE: ProcessSe(); break;
                case Token.SS: DhAndMixKey(_s, _rs); break;
            }
        }

        var bytesRead = _state.DecryptAndHash(message, payloadBuffer);
        Debug.Assert(bytesRead == plaintextSize);

        byte[]? handshakeHash = null;
        Encryption.Transport? transport = null;

        if (_messagePatterns.Count == 0)
        {
            (handshakeHash, transport) = Split();
        }

        _turnToWrite = true;
        return (plaintextSize, handshakeHash, transport);
    }

    private void ProcessPreMessages()
    {
        foreach (var token in s_handshakePattern.Initiator.Tokens)
        {
            if (token == Token.S)
            {
                _state.MixHash(_role == Role.ALICE ? _s.PublicKeyBytes : _rs);
            }
        }

        foreach (var token in s_handshakePattern.Responder.Tokens)
        {
            if (token == Token.S)
            {
                _state.MixHash(_role == Role.ALICE ? _rs : _s.PublicKeyBytes);
            }
        }
    }

    private void EnqueueMessages()
    {
        foreach (var pattern in s_handshakePattern.Patterns)
        {
            _messagePatterns.Enqueue(pattern);
        }
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
        _re = buffer[..CryptoConstants.PUBKEY_LEN].ToArray();
        _state.MixHash(_re);

        return buffer[_re.Length..];
    }

    private ReadOnlySpan<byte> ReadS(ReadOnlySpan<byte> message)
    {
        // Check version
        if (message[0] != HANDSHAKE_VERSION)
        {
            throw new InvalidOperationException("Invalid handshake version.");
        }
        message = message[1..];

        var length = _state.HasKeys() ? CryptoConstants.PUBKEY_LEN + CryptoConstants.CHACHA20_POLY1305_TAG_LEN : CryptoConstants.PUBKEY_LEN;
        var temp = message[..length];

        _rs = new byte[CryptoConstants.PUBKEY_LEN];
        _state.DecryptAndHash(temp, _rs);

        return message[length..];
    }

    private void ProcessEs()
    {
        if (_role == Role.ALICE)
        {
            DhAndMixKey(_e, _rs);
        }
        else
        {
            DhAndMixKey(_s, _re);
        }
    }

    private void ProcessSe()
    {
        if (_role == Role.ALICE)
        {
            DhAndMixKey(_s, _re);
        }
        else
        {
            DhAndMixKey(_e, _rs);
        }
    }

    private (byte[], Encryption.Transport) Split()
    {
        var (c1, c2) = _state.Split();

        var handshakeHash = _state.GetHandshakeHash();
        var transport = new Encryption.Transport(_role == _initiator, c1, c2);

        Clear();

        return (handshakeHash, transport);
    }

    private void DhAndMixKey(KeyPair? keyPair, ReadOnlySpan<byte> publicKey)
    {
        Debug.Assert(keyPair != null);
        Debug.Assert(!publicKey.IsEmpty);

        Span<byte> sharedKey = stackalloc byte[CryptoConstants.PRIVKEY_LEN];
        _dh.SecP256K1Dh(keyPair.PrivateKey, publicKey, sharedKey);
        _state.MixKey(sharedKey);
    }

    private void Clear()
    {
        _state.Dispose();
        _e?.Dispose();
        _s.Dispose();
    }

    private enum Role
    {
        ALICE,
        BOB
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Clear();
        GC.SuppressFinalize(this);

        _disposed = true;
    }

    ~HandshakeState()
    {
        Dispose();
    }
}