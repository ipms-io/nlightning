using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Messages.Interfaces;

namespace NLightning.Domain.Serialization.Interfaces;

public interface IMessageTypeSerializerFactory
{
    IMessageTypeSerializer<TMessageType>? GetSerializer<TMessageType>() where TMessageType : IMessage;
    IMessageTypeSerializer? GetSerializer(MessageTypes messageType);
}