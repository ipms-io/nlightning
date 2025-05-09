namespace NLightning.Application.Interfaces.Serialization;

using Domain.Protocol.Interfaces;

public interface IPayloadSerializer
{
    Task SerializeAsync(IMessagePayload payload, Stream stream);
    Task<TPayload> DeserializeAsync<TPayload>(Stream stream) where TPayload : IMessagePayload;
}