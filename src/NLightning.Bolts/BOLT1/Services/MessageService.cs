namespace NLightning.Bolts.BOLT1.Services;

using Bolts.Factories;
using Common.Interfaces;

/// <summary>
/// Service for sending and receiving messages.
/// </summary>
/// <remarks>
/// This class is used to send and receive messages over a transport service.
/// </remarks>
/// <seealso cref="IMessageService" />
internal sealed class MessageService : IMessageService
{
    private readonly ITransportService? _transportService;

    private bool _disposed;

    /// <inheritdoc />
    public event EventHandler<IMessage?>? MessageReceived;
    public event EventHandler<Exception>? ExceptionRaised;

    /// <inheritdoc />
    public bool IsConnected => _transportService?.IsConnected ?? false;

    /// <summary>
    /// Initializes a new <see cref="MessageService"/> class.
    /// </summary>
    /// <param name="transportService">The transport service.</param>
    public MessageService(ITransportService transportService)
    {
        _transportService = transportService;

        _transportService.MessageReceived += ReceiveMessage;
        _transportService.ExceptionRaised += RaiseException;
    }

    /// <inheritdoc />
    /// <exception cref="ObjectDisposedException">Thrown when the object is disposed.</exception>
    public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(MessageService));
        if (_transportService == null)
        {
            throw new InvalidOperationException($"{nameof(MessageService)} is not initialized");
        }

        await _transportService.WriteMessageAsync(message, cancellationToken);
    }

    private void ReceiveMessage(object? _, MemoryStream stream)
    {
        try
        {
            MessageFactory.DeserializeMessageAsync(stream).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    RaiseException(this, task.Exception.InnerExceptions[0]);
                }
                else
                {
                    var message = task.Result;
                    if (message is not null)
                    {
                        MessageReceived?.Invoke(this, message);
                    }
                }
            });
        }
        catch (Exception e)
        {
            RaiseException(this, e);
        }
    }

    private void RaiseException(object? sender, Exception e)
    {
        ExceptionRaised?.Invoke(sender, e);
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
        if (!_disposed)
        {
            if (disposing)
            {
                _transportService?.Dispose();
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