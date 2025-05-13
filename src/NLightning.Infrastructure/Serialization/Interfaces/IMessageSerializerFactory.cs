using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Infrastructure.Serialization.Interfaces;

public interface IMessageSerializerFactory
{
    Task SerializeAsync(IMessage message, Stream stream);
    Task<TMessage> DeserializeAsync<TMessage>(Stream stream, ushort messageType) where TMessage : IMessage;
}