namespace NLightning.Domain.Serialization.Messages;

using Protocol.Messages.Interfaces;

public interface IMessageSerializer
{
    Task SerializeAsync<TMessage>(TMessage message, Stream stream) where TMessage : IMessage;
    Task<TMessage?> DeserializeMessageAsync<TMessage>(Stream stream) where TMessage : class, IMessage;
    Task<IMessage?> DeserializeMessageAsync(Stream stream);
}