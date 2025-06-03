using NLightning.Domain.Protocol.Payloads.Interfaces;

namespace NLightning.Domain.Serialization.Interfaces;

/// <summary>
/// Interface for serializers that handle specific message types
/// </summary>
public interface IPayloadSerializer
{
    Task SerializeAsync(IMessagePayload payload, Stream stream);
    Task<IMessagePayload?> DeserializeAsync(Stream stream);
}

/// <summary>
/// Generic version for type safety
/// </summary>
public interface IPayloadSerializer<TMessagePayload>
    : IPayloadSerializer where TMessagePayload : IMessagePayload
{
    new Task<TMessagePayload?> DeserializeAsync(Stream stream);
}