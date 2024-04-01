namespace NLightning.Bolts.BOLT1.Services;

using Bolts.Interfaces;
using Factories;
using Interfaces;
using BOLT8.Interfaces;

internal sealed class MessageService : IMessageService
{
    private readonly ITransportService _transportService;

    private bool _disposed;

    public event EventHandler<IMessage>? MessageReceived;
    public bool IsConnected => _transportService.IsConnected;

    public MessageService(ITransportService transportService)
    {
        _transportService = transportService;

        _transportService.MessageReceived += (sender, message) =>
        {
            _ = ReceiveMessageAsync(message);
        };
    }

    public async Task SendMessageAsync(IMessage message)
    {
        await _transportService.WriteMessageAsync(message);
    }

    private async Task ReceiveMessageAsync(MemoryStream stream)
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