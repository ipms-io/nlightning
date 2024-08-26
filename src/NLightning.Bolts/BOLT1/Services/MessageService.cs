namespace NLightning.Bolts.BOLT1.Services;

using BOLT8.Interfaces;
using Bolts.Factories;
using Bolts.Interfaces;
using Interfaces;

/// <summary>
/// Service for sending and receiving messages.
/// </summary>
/// <remarks>
/// This class is used to send and receive messages over a transport service.
/// </remarks>
/// <seealso cref="IMessageService" />
internal sealed class MessageService : IMessageService
{
    private readonly ITransportService _transportService;

    private bool _disposed;

    /// <inheritdoc />
    public event EventHandler<IMessage>? MessageReceived;

    /// <inheritdoc />
    public bool IsConnected => _transportService.IsConnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageService"/> class.
    /// </summary>
    /// <param name="transportService">The transport service.</param>
    public MessageService(ITransportService transportService)
    {
        _transportService = transportService;

        _transportService.MessageReceived += ReceiveMessageAsync;
    }

    /// <inheritdoc />
    /// <exception cref="ObjectDisposedException">Thrown when the object is disposed.</exception>
    public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        ExceptionUtils.ThrowIfDisposed(_disposed, nameof(MessageService));

        await _transportService.WriteMessageAsync(message, cancellationToken);
    }

    private async void ReceiveMessageAsync(object? _, MemoryStream stream)
    {
        var message = await MessageFactory.DeserializeMessageAsync(stream);

        MessageReceived?.Invoke(this, message);
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