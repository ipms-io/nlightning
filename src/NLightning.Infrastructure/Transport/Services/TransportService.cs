using System.Buffers;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using NLightning.Domain.Serialization.Interfaces;
using NLightning.Infrastructure.Crypto.Interfaces;

namespace NLightning.Infrastructure.Transport.Services;

using Domain.Crypto.ValueObjects;
using Domain.Exceptions;
using Domain.Protocol.Messages.Interfaces;
using Domain.Transport;
using Exceptions;
using Interfaces;
using Protocol.Constants;

internal sealed class TransportService : ITransportService
{
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger _logger;
    private readonly IMessageSerializer _messageSerializer;
    private readonly TimeSpan _networkTimeout;
    private readonly SemaphoreSlim _networkWriteSemaphore = new(1, 1);
    private readonly TcpClient _tcpClient;
    private readonly TaskCompletionSource<bool> _tcs = new();

    private IHandshakeService? _handshakeService;
    private ITransport? _transport;
    private bool _disposed;

    // event that will be called when a message is received
    public event EventHandler<MemoryStream>? MessageReceived;
    public event EventHandler<Exception>? ExceptionRaised;

    public bool IsInitiator { get; }
    public bool IsConnected => _tcpClient.Connected;
    public CompactPubKey? RemoteStaticPublicKey { get; private set; }

    public TransportService(IEcdh ecdh, ILogger logger, IMessageSerializer messageSerializer, TimeSpan networkTimeout,
                            bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, TcpClient tcpClient)
        : this(logger, messageSerializer, networkTimeout, new HandshakeService(isInitiator, s, rs, null, ecdh),
               tcpClient)
    {
        _messageSerializer = messageSerializer;
        _networkTimeout = networkTimeout;
    }

    internal TransportService(ILogger logger, IMessageSerializer messageSerializer, TimeSpan networkTimeout,
                              IHandshakeService handshakeService, TcpClient tcpClient)
    {
        _handshakeService = handshakeService;
        _logger = logger;
        _messageSerializer = messageSerializer;
        _networkTimeout = networkTimeout;
        _tcpClient = tcpClient;

        IsInitiator = handshakeService.IsInitiator;
    }

    public async Task InitializeAsync()
    {
        if (_handshakeService == null)
            throw new NullReferenceException(nameof(_handshakeService));

        if (!_tcpClient.Connected)
            throw new InvalidOperationException("TcpClient is not connected");

        var writeBuffer = new byte[50];
        var stream = _tcpClient.GetStream();

        CancellationTokenSource networkTimeoutCancellationTokenSource = new();

        if (_handshakeService.IsInitiator)
        {
            try
            {
                _logger.LogTrace("We're initiator, writing Act One");

                // Write Act One
                var len = _handshakeService.PerformStep(ProtocolConstants.EmptyMessage, writeBuffer, out _);
                await stream.WriteAsync(writeBuffer.AsMemory()[..len], networkTimeoutCancellationTokenSource.Token);
                await stream.FlushAsync(networkTimeoutCancellationTokenSource.Token);

                // Read exactly 50 bytes
                _logger.LogTrace("Reading Act Two");
                networkTimeoutCancellationTokenSource = new CancellationTokenSource(_networkTimeout);
                var readBuffer = new byte[50];
                await stream.ReadExactlyAsync(readBuffer, networkTimeoutCancellationTokenSource.Token);
                networkTimeoutCancellationTokenSource.Dispose();

                // Read Act Two and Write Act Three
                _logger.LogTrace("Writing Act Three");
                writeBuffer = new byte[66];
                len = _handshakeService.PerformStep(readBuffer, writeBuffer, out _transport);
                await stream.WriteAsync(writeBuffer.AsMemory()[..len], CancellationToken.None);
                await stream.FlushAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                throw new ConnectionTimeoutException("Timeout while reading Handhsake's Act 2", e);
            }
        }
        else
        {
            var act = 1;

            try
            {
                _logger.LogTrace("We're NOT initiator, reading Act One");

                // Read exactly 50 bytes
                networkTimeoutCancellationTokenSource = new CancellationTokenSource(_networkTimeout);
                var readBuffer = new byte[50];
                await stream.ReadExactlyAsync(readBuffer, networkTimeoutCancellationTokenSource.Token);

                // Read Act One and Write Act Two
                _logger.LogTrace("Writing Act Two");
                var len = _handshakeService.PerformStep(readBuffer, writeBuffer, out _);
                await stream.WriteAsync(writeBuffer.AsMemory()[..len], CancellationToken.None);
                await stream.FlushAsync(CancellationToken.None);

                // Read exactly 66 bytes
                _logger.LogTrace("Reading Act Three");
                act = 3;
                networkTimeoutCancellationTokenSource = new CancellationTokenSource(_networkTimeout);
                readBuffer = new byte[66];
                await stream.ReadExactlyAsync(readBuffer, networkTimeoutCancellationTokenSource.Token);
                networkTimeoutCancellationTokenSource.Dispose();

                // Read Act Three
                _ = _handshakeService.PerformStep(readBuffer, writeBuffer, out _transport);
            }
            catch (Exception e)
            {
                throw new ConnectionTimeoutException($"Timeout while reading Handhsake's Act {act}", e);
            }
        }

        // Handshake completed
        if (_transport is null)
        {
            throw new InvalidOperationException("Handshake not completed");
        }
        RemoteStaticPublicKey = _handshakeService.RemoteStaticPublicKey
                                ?? throw new InvalidOperationException("RemoteStaticPublicKey is null");

        // Listen to messages and raise event
        _logger.LogTrace("Handshake completed, listening to messages");
        _ = Task.Run(ReadResponseAsync, CancellationToken.None).ContinueWith(task =>
        {
            if (task.Exception?.InnerExceptions.Count > 0)
                ExceptionRaised?.Invoke(this, task.Exception.InnerExceptions[0]);
        }, TaskContinuationOptions.OnlyOnFaulted);

        // Dispose of the handshake service
        _handshakeService.Dispose();
        _handshakeService = null;
    }

    public async Task WriteMessageAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        if (_tcpClient is null || !_tcpClient.Connected)
            throw new InvalidOperationException("TcpClient is not connected");

        if (_transport is null)
            throw new InvalidOperationException("Handshake not completed");

        // Serialize message
        using var messageStream = new MemoryStream();
        await _messageSerializer.SerializeAsync(message, messageStream);

        // Encrypt message
        var buffer = new byte[ProtocolConstants.MaxMessageLength];
        var size = _transport.WriteMessage(messageStream.ToArray(), buffer);

        // Write the message to stream
        await _networkWriteSemaphore.WaitAsync(cancellationToken);
        try
        {
            var stream = _tcpClient.GetStream();
            await stream.WriteAsync(buffer.AsMemory()[..size], cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        finally
        {
            _networkWriteSemaphore.Release();
        }
    }

    private async Task ReadResponseAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(ProtocolConstants.MaxMessageLength);
            var memoryBuffer = buffer.AsMemory();

            try
            {
                if (_transport == null)
                    throw new InvalidOperationException("Handshake not completed");

                if (_tcpClient is null || !_tcpClient.Connected)
                    throw new InvalidOperationException("TcpClient is not connected");

                // Read response
                var stream = _tcpClient.GetStream();
                var lenRead = await stream.ReadAsync(memoryBuffer[..ProtocolConstants.MessageHeaderSize], _cts.Token);
                if (lenRead != ProtocolConstants.MessageHeaderSize)
                    throw new ConnectionException("Peer sent wrong length");

                var messageLen = _transport.ReadMessageLength(memoryBuffer[..ProtocolConstants.MessageHeaderSize].Span);
                if (messageLen > ProtocolConstants.MaxMessageLength)
                    throw new ConnectionException("Peer sent message too long");

                lenRead = await stream.ReadAsync(memoryBuffer[..messageLen], _cts.Token);
                if (lenRead != messageLen)
                    throw new ConnectionException("Peer sent wrong body length");

                messageLen = _transport.ReadMessagePayload(memoryBuffer[..lenRead].Span, buffer);

                // Raise event
                var messageStream = new MemoryStream(buffer[..messageLen]);
                MessageReceived?.Invoke(this, messageStream);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (ConnectionException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (!_cts.IsCancellationRequested)
                {
                    if (_tcpClient is null || !_tcpClient.Connected)
                        throw new ConnectionException("Peer closed the connection");

                    throw new ConnectionException("Error reading response", e);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer, true);
            }
        }

        _tcs.TrySetResult(true);
    }

    #region Dispose Pattern
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Cancel and wait for an elegant shutdown
            _cts.Cancel();
            _tcs.Task.Wait(TimeSpan.FromSeconds(5));

            _handshakeService?.Dispose();
            _transport?.Dispose();
            _tcpClient.Dispose();
        }

        _disposed = true;
    }

    ~TransportService()
    {
        Dispose(false);
    }
    #endregion
}