namespace NLightning.Bolts.BOLT1.Interfaces;

using Bolts.Interfaces;

public interface IMessageService : IDisposable
{
    Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default);

    event EventHandler<IMessage>? MessageReceived;
    bool IsConnected { get; }
}