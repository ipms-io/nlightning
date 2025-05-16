namespace NLightning.Domain.Serialization.Factories;

using Payloads;
using Protocol.Payloads.Interfaces;

public interface IPayloadTypeSerializerFactory
{
    IPayloadTypeSerializer? GetSerializer(ushort messageType);
    IPayloadTypeSerializer<TPayloadType>? GetSerializer<TPayloadType>() where TPayloadType : IMessagePayload;
}