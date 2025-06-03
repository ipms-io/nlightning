using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Payloads.Interfaces;

namespace NLightning.Domain.Serialization.Interfaces;

public interface IPayloadSerializerFactory
{
    IPayloadSerializer? GetSerializer(MessageTypes messageType);
    IPayloadSerializer<TPayloadType>? GetSerializer<TPayloadType>() where TPayloadType : IMessagePayload;
}