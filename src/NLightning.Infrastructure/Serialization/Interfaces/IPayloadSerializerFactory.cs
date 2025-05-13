using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Infrastructure.Serialization.Interfaces;

public interface IPayloadSerializerFactory
{
    Task SerializeAsync(IMessagePayload payload, Stream stream);
    Task<TPayload> DeserializeAsync<TPayload>(Stream stream) where TPayload : IMessagePayload;
}