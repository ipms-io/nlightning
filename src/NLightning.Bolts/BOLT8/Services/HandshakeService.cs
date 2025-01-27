namespace NLightning.Bolts.BOLT8.Services;

using Common.Crypto.Functions;
using Constants;
using Interfaces;
using States;

/// <summary>
/// Initializes a new instance of the <see cref="HandshakeService"/> class.
/// </summary>
/// <param name="isInitiator">If we are initiating the connection</param>
/// <param name="localStaticPrivateKey">Our local Private Key</param>
/// <param name="staticPublicKey">If we are initiating, the remote Public Key, else our local Public Key</param>
internal sealed class HandshakeService(bool isInitiator, ReadOnlySpan<byte> localStaticPrivateKey, ReadOnlySpan<byte> staticPublicKey, IHandshakeState? handshakeState = null) : IHandshakeService
{
    private readonly IHandshakeState _handshakeState = handshakeState ?? new HandshakeState(isInitiator, localStaticPrivateKey, staticPublicKey, new Ecdh());

    private byte _steps = 2;

    /// <inheritdoc/>
    public bool IsInitiator => isInitiator;

    /// <inheritdoc/>
    public ITransport? Transport { get; private set; }

    public NBitcoin.PubKey? RemoteStaticPublicKey => _handshakeState.RemoteStaticPublicKey;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when there's no more steps to complete</exception>
    public int PerformStep(ReadOnlySpan<byte> inMessage, Span<byte> outMessage)
    {
        if (_steps == 2)
        {
            _steps--;
            return IsInitiator
                ? InitiatorWriteActOne(outMessage)
                : ResponderReadActOneAndWriteActTwo(inMessage, outMessage);
        }

        if (_steps == 1)
        {
            _steps--;
            return IsInitiator
                ? InitiatorReadActTwoAndWriteActThree(inMessage, outMessage)
                : ResponderReadActThree(inMessage);
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

    public void Dispose()
    {
        _handshakeState.Dispose();
    }
}