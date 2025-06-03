using NLightning.Domain.Protocol.Messages.Interfaces;

namespace NLightning.Domain.Serialization.Interfaces;

public interface IMessageSerializer
{
    Task SerializeAsync(IMessage message, Stream stream);
    Task<TMessage?> DeserializeMessageAsync<TMessage>(Stream stream) where TMessage : class, IMessage;
    Task<IMessage?> DeserializeMessageAsync(Stream stream);
}