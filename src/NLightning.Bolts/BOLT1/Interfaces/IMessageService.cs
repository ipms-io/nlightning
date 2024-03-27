namespace NLightning.Bolts.BOLT1.Interfaces;

using Bolts.Interfaces;

public interface IMessageService
{
    Task<IMessage> ReceiveMessageAsync(Stream networkStream);
    Task SendMessageAsync(IMessage message, Stream networkStream);
}