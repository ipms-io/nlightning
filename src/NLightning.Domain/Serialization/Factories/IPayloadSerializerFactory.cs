using NLightning.Domain.Serialization.Payloads;

namespace NLightning.Domain.Serialization.Factories;

using Protocol.Payloads.Interfaces;

public interface IPayloadSerializerFactory
{
    IPayloadSerializer? GetSerializer(ushort messageType);
    IPayloadSerializer<TPayloadType>? GetSerializer<TPayloadType>() where TPayloadType : IMessagePayload;
}