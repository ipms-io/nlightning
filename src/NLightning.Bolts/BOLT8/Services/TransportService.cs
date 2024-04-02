using System.Net.Sockets;

namespace NLightning.Bolts.BOLT8.Services;

using Bolts.Interfaces;
using Constants;
using Interfaces;
using NLightning.Bolts.Exceptions;
using static Common.Utils.Exceptions;

public sealed class TransportService : ITransportService
{
    private readonly TcpClient _tcpClient;
    private readonly IHandshakeService? _handshakeService;
    private readonly SemaphoreSlim _networkWriteSemaphore = new(1, 1);
    private readonly TaskCompletionSource<bool> _tcs = new();
    private readonly CancellationTokenSource _cts = new();

    private ITransport? _transport;
    private NBitcoin.PubKey? _remoteStaticPublicKey;
    private bool _disposed;

    // event that will be called when a message is received
    public event EventHandler<MemoryStream>? MessageReceived;

    public bool IsInitiator { get; }
    public bool IsConnected => _tcpClient.Connected;
    public NBitcoin.PubKey RemoteStaticPublicKey
    {
        get
        {
            ThrowIfDisposed(_disposed, nameof(TransportService));
            return _handshakeService?.RemoteStaticPublicKey ?? throw new InvalidOperationException("Handshake not completed");
        }
    }

    public TransportService(bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, TcpClient tcpClient) : this(new HandshakeService(isInitiator, s, rs), tcpClient)
    { }

    internal TransportService(IHandshakeService handshakeService, TcpClient tcpClient)
    {
        _handshakeService = handshakeService;
        _tcpClient = tcpClient;
        IsInitiator = handshakeService.IsInitiator;
    }

    public async Task InitializeAsync(TimeSpan networkTimeout)
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

        CancellationTokenSource networkTimeoutCancelationTokenSource = new();

        if (_handshakeService.IsInitiator)
        {
            try
            {
                // Write Act One
                var len = _handshakeService.PerformStep(ProtocolConstants.EMPTY_MESSAGE, writeBuffer);
                await stream.WriteAsync(writeBuffer.AsMemory()[..len]);
                await stream.FlushAsync();

                // Read exactly 50 bytes
                networkTimeoutCancelationTokenSource = new CancellationTokenSource(networkTimeout);
                var readBuffer = new byte[50];
                await stream.ReadExactlyAsync(readBuffer, networkTimeoutCancelationTokenSource.Token);
                networkTimeoutCancelationTokenSource.Dispose();

                // Read Act Two and Write Act Three
                writeBuffer = new byte[66];
                len = _handshakeService.PerformStep(readBuffer, writeBuffer);
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
                // Read exactly 50 bytes
                networkTimeoutCancelationTokenSource = new CancellationTokenSource(networkTimeout);
                var readBuffer = new byte[50];
                await stream.ReadExactlyAsync(readBuffer, networkTimeoutCancelationTokenSource.Token);

                // Read Act One and Write Act Two
                var len = _handshakeService.PerformStep(readBuffer, writeBuffer);
                await stream.WriteAsync(writeBuffer.AsMemory()[..len]);
                await stream.FlushAsync();

                // Read exactly 66 bytes
                act = 3;
                networkTimeoutCancelationTokenSource = new CancellationTokenSource(networkTimeout);
                readBuffer = new byte[66];
                await stream.ReadExactlyAsync(readBuffer, networkTimeoutCancelationTokenSource.Token);
                networkTimeoutCancelationTokenSource.Dispose();

                // Read Act Three
                _ = _handshakeService.PerformStep(readBuffer, writeBuffer);
            }
            catch (Exception e)
            {
                throw new ConnectionTimeoutException($"Timeout while reading Handhsake's Act {act}", e);
            }
        }

        // Listen to messages and raise event
        _ = Task.Run(ReadResponseAsync);

        // Handshake completed
        _transport = _handshakeService.Transport ?? throw new InvalidOperationException("Handshake not completed");
        _remoteStaticPublicKey = _handshakeService.RemoteStaticPublicKey;

        // Dispose of the handshake service
        _handshakeService.Dispose();
    }

    public async Task WriteMessageAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed(_disposed, nameof(TransportService));

        if (!_tcpClient.Connected)
        {
            throw new InvalidOperationException("TcpClient is not connected");
        }

        if (_transport == null)
        {
            throw new InvalidOperationException("Handshake not completed");
        }

        if (!IsInitiator)
        {
            throw new InvalidOperationException("Responder cannot write messages");
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
            if (_transport == null)
            {
                throw new InvalidOperationException("Handshake not completed");
            }

            if (!_tcpClient.Connected)
            {
                throw new InvalidOperationException("TcpClient is not connected");
            }

            // Read response
            var stream = _tcpClient.GetStream();
            var memory = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH].AsMemory();
            await stream.ReadAsync(memory[..ProtocolConstants.MESSAGE_HEADER_SIZE], _cts.Token);
            var messageLen = _transport.ReadMessageLength(memory.Span[..ProtocolConstants.MESSAGE_HEADER_SIZE]);
            await stream.ReadAsync(memory[..messageLen], _cts.Token);
            messageLen = _transport.ReadMessagePayload(memory.Span[..messageLen], memory.Span);

            // Raise event
            var messageStream = new MemoryStream(memory.Span[..messageLen].ToArray());
            MessageReceived?.Invoke(this, messageStream);
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
        if (!_disposed)
        {
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
    }

    ~TransportService()
    {
        Dispose(false);
    }
    #endregion
}