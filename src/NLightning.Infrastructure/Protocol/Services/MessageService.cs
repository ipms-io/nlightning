using Microsoft.Extensions.Logging;

namespace NLightning.Infrastructure.Protocol.Services;

using Domain.Exceptions;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Domain.Serialization.Interfaces;
using Domain.Transport;
using Exceptions;

/// <summary>
/// Service for sending and receiving messages.
/// </summary>
/// <remarks>
/// This class is used to send and receive messages over a transport service.
/// </remarks>
/// <seealso cref="IMessageService" />
internal sealed class MessageService : IMessageService
{
    private readonly ILogger<IMessageService> _logger;
    private readonly IMessageSerializer _messageSerializer;
    private readonly ITransportService? _transportService;

    private volatile bool _disposed;
    private readonly object _disposeLock = new();

    /// <inheritdoc />
    public event EventHandler<IMessage?>? OnMessageReceived;

    public event EventHandler<Exception>? OnExceptionRaised;

    /// <inheritdoc />
    public bool IsConnected => _transportService?.IsConnected ?? false;

    /// <summary>
    /// Initializes a new <see cref="MessageService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="messageSerializer">The message serializer.</param>
    /// <param name="transportService">The transport service.</param>
    public MessageService(ILogger<IMessageService> logger, IMessageSerializer messageSerializer,
                          ITransportService transportService)
    {
        _logger = logger;
        _messageSerializer = messageSerializer;
        _transportService = transportService;

        _transportService.MessageReceived += ReceiveMessage;
        _transportService.ExceptionRaised += RaiseException;
    }

    /// <inheritdoc />
    /// <exception cref="ObjectDisposedException">Thrown when the object is disposed.</exception>
    public async Task SendMessageAsync(IMessage message, bool throwOnException = false,
                                       CancellationToken cancellationToken = default)
    {
        lock (_disposeLock)
            if (_disposed)
            {
                var connectionException = new ConnectionException($"{nameof(MessageService)} was disposed.",
                                                                  new ObjectDisposedException(nameof(MessageService)));
                if (throwOnException)
                    throw connectionException;

                RaiseException(this, connectionException);
            }

        try
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (_transportService == null)
            {
                var connectionException = new ConnectionException($"{nameof(MessageService)} is not initialized");
                if (throwOnException)
                    throw connectionException;

                RaiseException(this, connectionException);
                return;
            }

            await _transportService.WriteMessageAsync(message, cancellationToken);
        }
        catch (Exception e)
        {
            var connectionException = new ConnectionException("Failed to send message", e);
            if (throwOnException)
                throw connectionException;

            RaiseException(this, connectionException);
        }
    }

    private void ReceiveMessage(object? _, MemoryStream stream)
    {
        try
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;

                var message = _messageSerializer.DeserializeMessageAsync(stream).GetAwaiter().GetResult();
                if (message is not null)
                {
                    OnMessageReceived?.Invoke(this, message);
                }
            }
        }
        catch (MessageSerializationException mse)
        {
            var message = mse.Message;
            if (mse.InnerException is PayloadSerializationException pse)
            {
                message = pse.InnerException is not null ? pse.InnerException.Message : pse.Message;
            }
            else if (mse.InnerException is not null)
            {
                message = mse.InnerException.Message;
            }

            _logger.LogError(mse, "Failed to deserialize message: {Message}", message);
            SendMessageAsync(new ErrorMessage(new ErrorPayload(message))).GetAwaiter().GetResult();
            RaiseException(this, mse);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to receive message");
            RaiseException(this, e);
        }
    }

    private void RaiseException(object? sender, Exception e)
    {
        OnExceptionRaised?.Invoke(sender, new ConnectionException("Error received from transportService", e));
    }

    #region Dispose Pattern

    /// <inheritdoc />
    /// <remarks>
    /// Disposes the TransportService.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        lock (_disposeLock)
        {
            if (_disposed)
                return;

            if (disposing && _transportService is not null)
            {
                _transportService.MessageReceived -= ReceiveMessage;
                _transportService.ExceptionRaised -= RaiseException;
                _transportService.Dispose();
            }

            _disposed = true;
        }
    }

    ~MessageService()
    {
        Dispose(false);
    }

    #endregion
}