using Microsoft.Extensions.Logging;

namespace NLightning.Infrastructure.Protocol.Services;

using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;
using Domain.Serialization.Interfaces;
using Domain.Transport;
using Domain.Utils;
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

    private bool _disposed;

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
    public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(MessageService));

        if (cancellationToken.IsCancellationRequested)
            return;

        if (_transportService == null)
            throw new InvalidOperationException($"{nameof(MessageService)} is not initialized");

        await _transportService.WriteMessageAsync(message, cancellationToken);
    }

    private void ReceiveMessage(object? _, MemoryStream stream)
    {
        try
        {
            var message = _messageSerializer.DeserializeMessageAsync(stream).GetAwaiter().GetResult();
            if (message is not null)
            {
                OnMessageReceived?.Invoke(this, message);
            }
        }
        catch (MessageSerializationException mse)
        {
            var message = mse.Message;
            if (mse.InnerException is PayloadSerializationException pse)
            {
                if (pse.InnerException is not null)
                    message = pse.InnerException.Message;
                else
                    message = pse.Message;
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
        OnExceptionRaised?.Invoke(sender, e);
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
        if (_disposed)
            return;

        if (disposing)
        {
            _transportService?.Dispose();
        }

        _disposed = true;
    }

    ~MessageService()
    {
        Dispose(false);
    }

    #endregion
}