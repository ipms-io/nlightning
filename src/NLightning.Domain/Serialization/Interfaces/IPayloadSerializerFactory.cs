namespace NLightning.Domain.Serialization.Interfaces;

using Protocol.Constants;
using Protocol.Interfaces;

public interface IPayloadSerializerFactory
{
    IPayloadSerializer? GetSerializer(MessageTypes messageType);
    IPayloadSerializer<TPayloadType>? GetSerializer<TPayloadType>() where TPayloadType : IMessagePayload;
}