namespace NLightning.Bolts.BOLT8.Services;

using Constants;
using Dhs;
using Interfaces;
using Primitives;
using States;

/// <summary>
/// Initializes a new instance of the <see cref="HandshakeService"/> class.
/// </summary>
/// <param name="isInitiator">If we are initiating the connection</param>
/// <param name="localStaticPrivateKey">Our local Private Key</param>
/// <param name="staticPublicKey">If we are initiating, the remote Public Key, else our local Public Key</param>
/// <param name="dh">A specific DH Function, or null to use the <see cref="Dhs.Secp256k1">Protocol Default</see></param>
internal sealed class HandshakeService(bool _isInitiator, ReadOnlySpan<byte> _localStaticPrivateKey, ReadOnlySpan<byte> _staticPublicKey, IHandshakeState? _handshakeState = null) : IHandshakeService
{
    private readonly IHandshakeState _handshakeState = _handshakeState ?? new HandshakeState(_isInitiator, _localStaticPrivateKey, _staticPublicKey, new Secp256k1());

    private byte _steps = 2;
    private bool _disposed;

    /// <inheritdoc/>
    public bool IsInitiator => _isInitiator;

    /// <inheritdoc/>
    public ITransport? Transport { get; private set; }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when there's no more steps to complete</exception>
    public int PerformStep(ReadOnlySpan<byte> inMessage, Span<byte> outMessage)
    {
        if (_steps == 2)
        {
            _steps--;
            if (_isInitiator)
            {
                return InitiatorWriteActOne(outMessage);
            }
            else
            {
                return ResponderReadActOneAndWriteActTwo(inMessage, outMessage);
            }
        }
        else if (_steps == 1)
        {
            _steps--;
            if (_isInitiator)
            {
                return InitiatorReadActTwoAndWriteActThree(inMessage, outMessage);
            }
            else
            {
                return ResponderReadActThree(inMessage);
            }
        }

        throw new InvalidOperationException("There's no more steps to complete");
    }

    #region Initiator Methods
    /// <summary>
    /// Initiator writes act one
    /// </summary>
    /// <param name="outMessage">The buffer to write the message to</param>
    /// <returns>Number of bytes written to outMessage</returns>
    private int InitiatorWriteActOne(Span<byte> outMessage)
    {
        // Write act one
        return _handshakeState.WriteMessage(ProtocolConstants.EMPTY_MESSAGE, outMessage).Item1;
    }

    /// <summary>
    /// Initiator reads act two and writes act three
    /// </summary>
    /// <param name="actTwoMessage">Byte[] representation of Act Two Message</param>
    /// <param name="outMessage">The buffer to write the message to</param>
    /// <returns>Number of bytes written to outMessage</returns>
    private int InitiatorReadActTwoAndWriteActThree(ReadOnlySpan<byte> actTwoMessage, Span<byte> outMessage)
    {
        int messageSize;

        // Read act two
        _ = _handshakeState.ReadMessage(actTwoMessage, outMessage);

        // Write act three
        (messageSize, _, Transport) = _handshakeState.WriteMessage(ProtocolConstants.EMPTY_MESSAGE, outMessage);

        return messageSize;
    }
    #endregion

    #region Responder Methods
    /// <summary>
    /// Responder reads act one and writes act two
    /// </summary>
    /// <param name="actOneMessage">Byte[] representation of Act One Message</param>
    /// <param name="outMessage">The buffer to write the message to</param>
    /// <returns>Number of bytes written to outMessage</returns>
    private int ResponderReadActOneAndWriteActTwo(ReadOnlySpan<byte> actOneMessage, Span<byte> outMessage)
    {
        // Read act one
        var (responderMessageSize, _, _) = _handshakeState.ReadMessage(actOneMessage, outMessage);

        // Write act two
        return _handshakeState.WriteMessage(outMessage[..responderMessageSize], outMessage).Item1;
    }

    /// <summary>
    /// Responder reads act three
    /// </summary>
    /// <param name="actThreeMessage">Byte[] representation of Act Three Message</param>
    /// <returns>Number of bytes read from actThreeMessage</returns>
    private int ResponderReadActThree(ReadOnlySpan<byte> actThreeMessage)
    {
        int messageSize;

        // Read act three
        var messageBuffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
        (messageSize, _, Transport) = _handshakeState.ReadMessage(actThreeMessage, messageBuffer);

        return messageSize;
    }
    #endregion

    #region Dispose Pattern
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _handshakeState.Dispose();
                Transport?.Dispose();
            }

            _disposed = true;
        }
    }

    ~HandshakeService()
    {
        Dispose(false);
    }
    #endregion
}