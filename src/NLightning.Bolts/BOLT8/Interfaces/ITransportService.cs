namespace NLightning.Bolts.BOLT8.Interfaces;

using Bolts.Interfaces;

public interface ITransportService : IDisposable
{
    Task InitializeAsync();
    Task WriteMessageAsync(IMessage message);
    Task DisconnectAsync();

    event EventHandler<MemoryStream>? MessageReceived;
    bool IsInitiator { get; }
    bool IsConnected { get; }
}