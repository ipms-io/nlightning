using System.Net.Sockets;

namespace NLightning.Bolts.BOLT8.Services;

using Interfaces;
using Constants;
using Common.Interfaces;
using static Common.Utils.Exceptions;

public sealed class TransportService : ITransportService
{
    private readonly TcpClient _tcpClient;
    private readonly bool _isInitiator;

    private IHandshakeService? _handshakeService;
    private ITransport? _transport;
    private bool _disposed;

    // event that will be called when a message is received
    public event EventHandler<byte[]>? MessageReceived;

    public TransportService(bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, TcpClient tcpClient) : this(new HandshakeService(isInitiator, s, rs), tcpClient)
    { }

    internal TransportService(IHandshakeService handshakeService, TcpClient tcpClient)
    {
        _handshakeService = handshakeService;
        _tcpClient = tcpClient;
        _isInitiator = handshakeService.IsInitiator;
    }

    public async Task Initialize()
    {
        ThrowIfDisposed(_disposed, nameof(TransportService));
        if (_handshakeService == null)
        {
            throw new NullReferenceException(nameof(_handshakeService));
        }

        if (!_tcpClient.Connected)
        {
            throw new InvalidOperationException("TcpClient is not connected");
        }

        using var stream = _tcpClient.GetStream();
        var writeBuffer = new byte[50];

        if (_handshakeService.IsInitiator)
        {
            // Write Act One
            var len = _handshakeService.PerformStep(ProtocolConstants.EMPTY_MESSAGE, writeBuffer);
            await stream.WriteAsync(writeBuffer.AsMemory()[..len]);

            // Read exactly 50 bytes
            var readBuffer = new byte[50];
            await stream.ReadExactlyAsync(readBuffer);

            // Read Act Two and Write Act Three
            writeBuffer = new byte[66];
            len = _handshakeService.PerformStep(readBuffer, writeBuffer);
            await stream.WriteAsync(writeBuffer.AsMemory()[..len]);
        }
        else
        {
            // Read exactly 50 bytes
            var readBuffer = new byte[50];
            await stream.ReadExactlyAsync(readBuffer);

            // Read Act One and Write Act Two
            var len = _handshakeService.PerformStep(readBuffer, writeBuffer);
            await stream.WriteAsync(writeBuffer.AsMemory()[..len]);

            // Read exactly 66 bytes
            readBuffer = [66];
            await stream.ReadExactlyAsync(readBuffer);

            // Read Act Three
            _ = _handshakeService.PerformStep(readBuffer, writeBuffer);

            // listenm to messages and raise event
            _ = Task.Run(() =>
            {
                while (!_disposed)
                {
                    ReadResponse();
                }
            });
        }

        // Handshake completed
        _transport = _handshakeService.Transport ?? throw new InvalidOperationException("Handshake not completed");

        // Dispose of the handshake service
        _handshakeService.Dispose();
        _handshakeService = null;
    }

    public void WriteMessage(IMessage message)
    {
        ThrowIfDisposed(_disposed, nameof(TransportService));
        if (_transport == null)
        {
            throw new InvalidOperationException("Handshake not completed");
        }

        if (!_isInitiator)
        {
            throw new InvalidOperationException("Responder cannot write messages");
        }

        // Encrypt message
        var buffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
        _transport.WriteMessage(message.Serialize(), buffer);

        // Write message to stream
        using var stream = _tcpClient.GetStream();
        stream.Write(buffer);

        // Read response
        ReadResponse(stream);
    }

    private void ReadResponse(NetworkStream stream)
    {
        ThrowIfDisposed(_disposed, nameof(TransportService));
        if (_transport == null)
        {
            throw new InvalidOperationException("Handshake not completed");
        }

        var buffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];

        // Read response
        stream.Read(buffer, 0, ProtocolConstants.MESSAGE_HEADER_SIZE);
        var messageLen = _transport.ReadMessageLength(buffer.AsSpan()[..ProtocolConstants.MESSAGE_HEADER_SIZE]);
        stream.Read(buffer, 0, messageLen);
        messageLen = _transport.ReadMessagePayload(buffer.AsSpan()[..messageLen], buffer);

        // Raise event
        MessageReceived?.Invoke(this, buffer[..messageLen]);
    }
    private void ReadResponse()
    {
        ThrowIfDisposed(_disposed, nameof(TransportService));
        if (_transport == null)
        {
            throw new InvalidOperationException("Handshake not completed");
        }

        using var stream = _tcpClient.GetStream();

        var buffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];

        // Read response
        stream.Read(buffer, 0, ProtocolConstants.MESSAGE_HEADER_SIZE);
        var messageLen = _transport.ReadMessageLength(buffer.AsSpan()[..ProtocolConstants.MESSAGE_HEADER_SIZE]);
        stream.Read(buffer, 0, messageLen);
        messageLen = _transport.ReadMessagePayload(buffer.AsSpan()[..messageLen], buffer);

        // Raise event
        MessageReceived?.Invoke(this, buffer[..messageLen]);
    }

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
                _handshakeService?.Dispose();
                _transport?.Dispose();
                _tcpClient.Dispose();
            }

            _disposed = true;
        }
    }

    ~TransportService()
    {
        Dispose(false);
    }
    #endregion
}