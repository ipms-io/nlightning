namespace NLightning.Domain.Serialization.Interfaces;

using Protocol.Interfaces;

public interface IMessageSerializer
{
    Task SerializeAsync(IMessage message, Stream stream);
    Task<TMessage?> DeserializeMessageAsync<TMessage>(Stream stream) where TMessage : class, IMessage;
    Task<IMessage?> DeserializeMessageAsync(Stream stream);
}