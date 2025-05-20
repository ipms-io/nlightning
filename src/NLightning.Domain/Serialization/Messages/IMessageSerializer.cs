namespace NLightning.Domain.Serialization.Messages;

using Protocol.Messages.Interfaces;

public interface IMessageSerializer
{
    Task SerializeAsync(IMessage message, Stream stream);
    Task<TMessage?> DeserializeMessageAsync<TMessage>(Stream stream) where TMessage : class, IMessage;
    Task<IMessage?> DeserializeMessageAsync(Stream stream);
}