using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Application.Interfaces.Serialization;

/// <summary>
/// Interface for serializers that handle specific payload types
/// </summary>
public interface IPayloadTypeSerializer
{
    Task SerializeAsync(IMessagePayload payload, Stream stream);
    Task<IMessagePayload> DeserializeAsync(Stream stream);
}

/// <summary>
/// Generic version for type safety
/// </summary>
public interface IPayloadTypeSerializer<TPayload> : IPayloadTypeSerializer where TPayload : IMessagePayload
{
    new Task<TPayload> DeserializeAsync(Stream stream);
}