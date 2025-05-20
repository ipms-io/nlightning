namespace NLightning.Domain.Serialization.Factories;

using Messages.Types;
using Protocol.Messages.Interfaces;

public interface IMessageTypeSerializerFactory
{
    IMessageTypeSerializer<TMessageType>? GetSerializer<TMessageType>() where TMessageType : IMessage;
    IMessageTypeSerializer? GetSerializer(ushort messageType);
}