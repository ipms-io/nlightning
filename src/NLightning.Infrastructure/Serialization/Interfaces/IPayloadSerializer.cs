using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Infrastructure.Serialization.Interfaces;

/// <summary>
/// Interface for serializers that handle specific payload types
/// </summary>
public interface IPayloadSerializer
{
    Task SerializeAsync(IMessagePayload payload, Stream stream);
    Task<IMessagePayload> DeserializeAsync(Stream stream);
}

/// <summary>
/// Generic version for type safety
/// </summary>
public interface IPayloadSerializer<TPayload> : IPayloadSerializer where TPayload : IMessagePayload
{
    new Task<TPayload> DeserializeAsync(Stream stream);
}