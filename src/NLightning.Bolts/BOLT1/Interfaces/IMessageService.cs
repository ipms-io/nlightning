namespace NLightning.Bolts.BOLT1.Interfaces;

using Bolts.Interfaces;

public interface IMessageService : IDisposable
{
    Task SendMessageAsync(IMessage message);

    event EventHandler<IMessage>? MessageReceived;
    bool IsConnected { get; }
}