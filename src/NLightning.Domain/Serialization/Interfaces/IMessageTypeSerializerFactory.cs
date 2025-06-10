namespace NLightning.Domain.Serialization.Interfaces;

using Protocol.Constants;
using Protocol.Interfaces;

public interface IMessageTypeSerializerFactory
{
    IMessageTypeSerializer<TMessageType>? GetSerializer<TMessageType>() where TMessageType : IMessage;
    IMessageTypeSerializer? GetSerializer(MessageTypes messageType);
}