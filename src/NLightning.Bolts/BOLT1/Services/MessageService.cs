namespace NLightning.Bolts.BOLT1.Services;

using BOLT8.Interfaces;
using Bolts.Interfaces;
using Factories;
using Interfaces;

internal sealed class MessageService : IMessageService
{
    private readonly ITransportService _transportService;

    private bool _disposed;

    public event EventHandler<IMessage>? MessageReceived;
    public bool IsConnected => _transportService.IsConnected;

    public MessageService(ITransportService transportService)
    {
        _transportService = transportService;

        _transportService.MessageReceived += ReceiveMessageAsync;
    }

    public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        await _transportService.WriteMessageAsync(message, cancellationToken);
    }

    private async void ReceiveMessageAsync(object? _, MemoryStream stream)
    {
        var message = await MessageFactory.DeserializeMessageAsync(stream);

        MessageReceived?.Invoke(this, message);
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