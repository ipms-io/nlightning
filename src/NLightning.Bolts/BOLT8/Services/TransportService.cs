using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace NLightning.Bolts.BOLT8.Services;

using Common.Constants;
using Common.Interfaces;
using Common.Managers;
using Interfaces;
using static ExceptionUtils;

internal sealed class TransportService : ITransportService
{
    private readonly SemaphoreSlim _networkWriteSemaphore = new(1, 1);
    private readonly TaskCompletionSource<bool> _tcs = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger _logger;

    private readonly TcpClient _tcpClient;
    private IHandshakeService? _handshakeService;

    private ITransport? _transport;
    private bool _disposed;

    // event that will be called when a message is received
    public event EventHandler<MemoryStream>? MessageReceived;
    public event EventHandler<Exception>? ExceptionRaised;

    public bool IsInitiator { get; }
    public bool IsConnected => _tcpClient.Connected;
    public NBitcoin.PubKey? RemoteStaticPublicKey { get; private set; }

    public TransportService(ILogger logger, bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, TcpClient tcpClient)
        : this(logger, new HandshakeService(isInitiator, s, rs), tcpClient)
    { }

    internal TransportService(ILogger logger, IHandshakeService handshakeService, TcpClient tcpClient)
    {
        _logger = logger;
        _handshakeService = handshakeService;
        _tcpClient = tcpClient;
        IsInitiator = handshakeService.IsInitiator;
    }

    public async Task InitializeAsync()
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

        var writeBuffer = new byte[50];
        var stream = _tcpClient.GetStream();

        CancellationTokenSource networkTimeoutCancellationTokenSource = new();

        if (_handshakeService.IsInitiator)
        {
            try
            {
                _logger.LogTrace("[TransportService] We're initiator, writing Act One");

                // Write Act One
                var len = _handshakeService.PerformStep(ProtocolConstants.EMPTY_MESSAGE, writeBuffer, out _);
                await stream.WriteAsync(writeBuffer.AsMemory()[..len], networkTimeoutCancellationTokenSource.Token);
                await stream.FlushAsync(networkTimeoutCancellationTokenSource.Token);

                // Read exactly 50 bytes
                _logger.LogTrace("[TransportService] Reading Act Two");
                networkTimeoutCancellationTokenSource = new CancellationTokenSource(ConfigManager.Instance.NetworkTimeout);
                var readBuffer = new byte[50];
                await stream.ReadExactlyAsync(readBuffer, networkTimeoutCancellationTokenSource.Token);
                networkTimeoutCancellationTokenSource.Dispose();

                // Read Act Two and Write Act Three
                _logger.LogTrace("[TransportService] Writing Act Three");
                writeBuffer = new byte[66];
                len = _handshakeService.PerformStep(readBuffer, writeBuffer, out _transport);
                await stream.WriteAsync(writeBuffer.AsMemory()[..len]);
                await stream.FlushAsync();
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
                _logger.LogTrace("[TransportService] We're NOT initiator, reading Act One");

                // Read exactly 50 bytes
                networkTimeoutCancellationTokenSource = new CancellationTokenSource(ConfigManager.Instance.NetworkTimeout);
                var readBuffer = new byte[50];
                await stream.ReadExactlyAsync(readBuffer, networkTimeoutCancellationTokenSource.Token);

                // Read Act One and Write Act Two
                _logger.LogTrace("[TransportService] Writing Act Two");
                var len = _handshakeService.PerformStep(readBuffer, writeBuffer, out _);
                await stream.WriteAsync(writeBuffer.AsMemory()[..len]);
                await stream.FlushAsync();

                // Read exactly 66 bytes
                _logger.LogTrace("[TransportService] Reading Act Three");
                act = 3;
                networkTimeoutCancellationTokenSource = new CancellationTokenSource(ConfigManager.Instance.NetworkTimeout);
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
        _logger.LogTrace("[TransportService] Handshake completed, listening to messages");
        _ = Task.Run(ReadResponseAsync, CancellationToken.None).ContinueWith(task =>
        {
            if (task.Exception?.InnerExceptions.Count > 0)
            {
                ExceptionRaised?.Invoke(this, task.Exception.InnerExceptions[0]);
            }
        }, TaskContinuationOptions.OnlyOnFaulted);

        // Dispose of the handshake service
        _handshakeService.Dispose();
        _handshakeService = null;
    }

    public async Task WriteMessageAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed(_disposed, nameof(TransportService));

        if (_tcpClient is null || !_tcpClient.Connected)
        {
            throw new InvalidOperationException("TcpClient is not connected");
        }

        if (_transport is null)
        {
            throw new InvalidOperationException("Handshake not completed");
        }

        // Serialize message
        using var messageStream = new MemoryStream();
        await message.SerializeAsync(messageStream);

        // Encrypt message
        var buffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
        var size = _transport.WriteMessage(messageStream.ToArray(), buffer);

        // Write message to stream
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
        ThrowIfDisposed(_disposed, nameof(TransportService));

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                ThrowIfDisposed(_disposed, nameof(TransportService));
                if (_transport == null)
                {
                    throw new InvalidOperationException("Handshake not completed");
                }

                if (_tcpClient is null || !_tcpClient.Connected)
                {
                    throw new InvalidOperationException("TcpClient is not connected");
                }

                // Read response
                var stream = _tcpClient.GetStream();
                var memory = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH].AsMemory();
                _ = await stream.ReadAsync(memory[..ProtocolConstants.MESSAGE_HEADER_SIZE], _cts.Token);
                var messageLen = _transport.ReadMessageLength(memory.Span[..ProtocolConstants.MESSAGE_HEADER_SIZE]);
                _ = await stream.ReadAsync(memory[..messageLen], _cts.Token);
                messageLen = _transport.ReadMessagePayload(memory.Span[..messageLen], memory.Span);

                // Raise event
                var messageStream = new MemoryStream(memory.Span[..messageLen].ToArray());
                MessageReceived?.Invoke(this, messageStream);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (Exception e)
            {
                if (!_cts.IsCancellationRequested)
                {
                    throw new ConnectionException("Error reading response", e);
                }
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