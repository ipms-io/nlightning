namespace NLightning.Domain.Serialization.Factories;

using Payloads;
using Protocol.Constants;
using Protocol.Payloads.Interfaces;

public interface IPayloadSerializerFactory
{
    IPayloadSerializer? GetSerializer(MessageTypes messageType);
    IPayloadSerializer<TPayloadType>? GetSerializer<TPayloadType>() where TPayloadType : IMessagePayload;
}