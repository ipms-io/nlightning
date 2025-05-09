namespace NLightning.Application.Interfaces.Serialization;

using Domain.Protocol.Interfaces;

public interface IMessageSerializer
{
    Task SerializeAsync(IMessage message, Stream stream);
    Task<TMessage> DeserializeAsync<TMessage>(Stream stream, ushort messageType) where TMessage : IMessage;
}