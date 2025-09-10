using System.Diagnostics;

namespace NLightning.Infrastructure.Transport.Handshake.States;

using Crypto.Interfaces;
using Domain.Crypto.Constants;
using Domain.Crypto.ValueObjects;
using Domain.Utils;
using Enums;
using Interfaces;
using MessagePatterns;
using Protocol.Constants;

/// <inheritdoc/>
/// <remarks> See <see href="https://github.com/lightning/bolts/blob/master/08-transport.md">Lightning Bolt8</see> for specific implementation information.</remarks>
internal sealed class HandshakeState : IHandshakeState
{
    private const byte HandshakeVersion = 0x00;
    private static readonly HandshakePattern s_handshakePattern = HandshakePattern.Xk;

    private readonly SymmetricState _state;
    private readonly Role _role;
    private readonly Role _initiator;
    private readonly CryptoKeyPair _s;
    private readonly Queue<MessagePattern> _messagePatterns = new();

    private readonly IEcdh _dh;
    private CryptoKeyPair? _e;
    private byte[]? _re;
    private byte[] _rs;
    private bool _turnToWrite;
    private bool _disposed;

    public CompactPubKey? RemoteStaticPublicKey => new(_rs);

    /// <summary>
    /// Creates a new HandshakeState instance.
    /// </summary>
    /// <param name="initiator">If we are the initiator</param>
    /// <param name="s">Local Static Private Key</param>
    /// <param name="rs">Remote Static Public Key</param>
    /// <param name="dh">A specific DH Function</param>
    /// <exception cref="ArgumentException"></exception>
    public HandshakeState(bool initiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, IEcdh dh)
    {
        if (s.IsEmpty)
            throw new ArgumentException("Local static private key required, but not provided.", nameof(s));

        if (s.Length != CryptoConstants.PrivkeyLen)
            throw new ArgumentException("Invalid local static private key.", nameof(s));

        if (rs.IsEmpty)
            throw new ArgumentException("Remote static public key required, but not provided.", nameof(rs));

        if (rs.Length != CryptoConstants.CompactPubkeyLen)
            throw new ArgumentException("Invalid remote static public key.", nameof(rs));

        ArgumentNullException.ThrowIfNull(dh, nameof(dh));

        _dh = dh;

        _state = new SymmetricState(ProtocolConstants.Name);
        _state.MixHash(ProtocolConstants.Prologue);

        _role = initiator ? Role.Alice : Role.Bob;
        _initiator = Role.Alice;
        _turnToWrite = initiator;
        _s = _dh.GenerateKeyPair(s);
        _rs = rs.ToArray();

        ProcessPreMessages();
        EnqueueMessages();
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">Thrown if the current instance has already been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the call to <see cref="ReadMessage"/> was expected or the handshake has already been completed.</exception>
    /// <exception cref="ArgumentException">Thrown if the output was greater than <see cref="ProtocolConstants.MaxMessageLength"/> bytes in length, or if the output buffer did not have enough space to hold the ciphertext.</exception>
    public (int, byte[]?, Encryption.Transport?) WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer)
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(HandshakeState));

        if (_messagePatterns.Count == 0)
            throw new InvalidOperationException(
                "Cannot call WriteMessage after the handshake has already been completed.");

        var overhead = _messagePatterns.Peek().Overhead(CryptoConstants.CompactPubkeyLen, _state.HasKeys());
        var ciphertextSize = payload.Length + overhead;

        if (ciphertextSize > ProtocolConstants.MaxMessageLength)
            throw new ArgumentException(
                $"Noise message must be less than or equal to {ProtocolConstants.MaxMessageLength} bytes in length.");

        if (ciphertextSize > messageBuffer.Length)
            throw new ArgumentException("Message buffer does not have enough space to hold the ciphertext.");

        if (!_turnToWrite)
            throw new InvalidOperationException("Unexpected call to WriteMessage (should be ReadMessage).");

        var next = _messagePatterns.Dequeue();
        var messageBufferLength = messageBuffer.Length;

        // write the version to the message buffer
        messageBuffer[0] = HandshakeVersion;

        foreach (var token in next.Tokens)
        {
            switch (token)
            {
                case Token.E: messageBuffer = WriteE(messageBuffer); break;
                case Token.S: messageBuffer = WriteS(messageBuffer); break;
                case Token.Ee: DhAndMixKey(_e, _re); break;
                case Token.Es: ProcessEs(); break;
                case Token.Se: ProcessSe(); break;
                case Token.Ss: DhAndMixKey(_s, _rs); break;
            }
        }

        var bytesWritten = _state.EncryptAndHash(payload, messageBuffer);
        var size = messageBufferLength - messageBuffer.Length + bytesWritten;

        Debug.Assert(ciphertextSize == size);

        byte[]? handshakeHash = null;
        Encryption.Transport? transport = null;

        if (_messagePatterns.Count == 0)
            (handshakeHash, transport) = Split();

        _turnToWrite = false;
        return (ciphertextSize, handshakeHash, transport);
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">Thrown if the current instance has already been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the call to <see cref="WriteMessage"/> was expected or the handshake has already been completed.</exception>
    /// <exception cref="ArgumentException">Thrown if the message was greater than <see cref="ProtocolConstants.MaxMessageLength"/> bytes in length, or if the output buffer did not have enough space to hold the plaintext.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown if the decryption of the message has failed.</exception>
    public (int, byte[]?, Encryption.Transport?) ReadMessage(ReadOnlySpan<byte> message, Span<byte> payloadBuffer)
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(HandshakeState));

        if (_messagePatterns.Count == 0)
            throw new InvalidOperationException(
                "Cannot call WriteMessage after the handshake has already been completed.");

        var overhead = _messagePatterns.Peek().Overhead(CryptoConstants.CompactPubkeyLen, _state.HasKeys());
        var plaintextSize = message.Length - overhead;

        if (message.Length > ProtocolConstants.MaxMessageLength)
            throw new ArgumentException(
                $"Noise message must be less than or equal to {ProtocolConstants.MaxMessageLength} bytes in length.");

        if (message.Length != overhead)
            throw new ArgumentException($"Noise message must be equal to {overhead} bytes in length.");

        if (plaintextSize > payloadBuffer.Length)
            throw new ArgumentException("Payload buffer does not have enough space to hold the plaintext.");

        if (_turnToWrite)
            throw new InvalidOperationException("Unexpected call to ReadMessage (should be WriteMessage).");

        var next = _messagePatterns.Dequeue();
        foreach (var token in next.Tokens)
        {
            switch (token)
            {
                case Token.E: message = ReadE(message); break;
                case Token.S: message = ReadS(message); break;
                case Token.Ee: DhAndMixKey(_e, _re); break;
                case Token.Es: ProcessEs(); break;
                case Token.Se: ProcessSe(); break;
                case Token.Ss: DhAndMixKey(_s, _rs); break;
            }
        }

        var bytesRead = _state.DecryptAndHash(message, payloadBuffer);
        Debug.Assert(bytesRead == plaintextSize);

        byte[]? handshakeHash = null;
        Encryption.Transport? transport = null;

        if (_messagePatterns.Count == 0)
            (handshakeHash, transport) = Split();

        _turnToWrite = true;
        return (plaintextSize, handshakeHash, transport);
    }

    private void ProcessPreMessages()
    {
        foreach (var token in s_handshakePattern.Initiator.Tokens)
        {
            if (token == Token.S)
            {
                _state.MixHash(_role == Role.Alice ? _s.CompactPubKey : _rs);
            }
        }

        foreach (var token in s_handshakePattern.Responder.Tokens)
        {
            if (token == Token.S)
            {
                _state.MixHash(_role == Role.Alice ? _rs : _s.CompactPubKey);
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
        ((ReadOnlySpan<byte>)_e.Value.CompactPubKey).CopyTo(buffer[1..]);
        _state.MixHash(_e.Value.CompactPubKey);

        // Remember to add our version length to the resulting Span
        return buffer[(CryptoConstants.CompactPubkeyLen + 1)..];
    }

    private Span<byte> WriteS(Span<byte> buffer)
    {
        // Start from position 1, since we need our version there
        var bytesWritten = _state.EncryptAndHash(_s.CompactPubKey, buffer[1..]);

        // Don't forget to add our version length to the resulting Span
        return buffer[(bytesWritten + 1)..];
    }

    private ReadOnlySpan<byte> ReadE(ReadOnlySpan<byte> buffer)
    {
        Debug.Assert(_re == null);

        // Check version
        if (buffer[0] != HandshakeVersion)
        {
            throw new InvalidOperationException("Invalid handshake version.");
        }

        buffer = buffer[1..];

        // Skip the byte from the version and get all bytes from pubkey
        _re = buffer[..CryptoConstants.CompactPubkeyLen].ToArray();
        _state.MixHash(_re);

        return buffer[_re.Length..];
    }

    private ReadOnlySpan<byte> ReadS(ReadOnlySpan<byte> message)
    {
        // Check version
        if (message[0] != HandshakeVersion)
        {
            throw new InvalidOperationException("Invalid handshake version.");
        }

        message = message[1..];

        var length = _state.HasKeys()
                         ? CryptoConstants.CompactPubkeyLen + CryptoConstants.Chacha20Poly1305TagLen
                         : CryptoConstants.CompactPubkeyLen;
        var temp = message[..length];

        _rs = new byte[CryptoConstants.CompactPubkeyLen];
        _state.DecryptAndHash(temp, _rs);

        return message[length..];
    }

    private void ProcessEs()
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

    private void ProcessSe()
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

    private (byte[], Encryption.Transport) Split()
    {
        var (c1, c2) = _state.Split();

        var handshakeHash = _state.GetHandshakeHash();
        var transport = new Encryption.Transport(_role == _initiator, c1, c2);

        Clear();

        return (handshakeHash, transport);
    }

    private void DhAndMixKey(CryptoKeyPair? keyPair, ReadOnlySpan<byte> publicKey)
    {
        Debug.Assert(keyPair != null);
        Debug.Assert(!publicKey.IsEmpty);

        Span<byte> sharedKey = stackalloc byte[CryptoConstants.PrivkeyLen];
        _dh.SecP256K1Dh(keyPair.Value.PrivKey, publicKey, sharedKey);
        _state.MixKey(sharedKey);
    }

    private void Clear()
    {
        _state.Dispose();
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