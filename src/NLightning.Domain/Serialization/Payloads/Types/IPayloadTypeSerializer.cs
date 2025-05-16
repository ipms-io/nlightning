namespace NLightning.Domain.Serialization.Payloads.Types;

using Protocol.Payloads.Interfaces;

/// <summary>
/// Interface for serializers that handle specific message types
/// </summary>
public interface IPayloadTypeSerializer
{
    Task SerializeAsync(IMessagePayload payload, Stream stream);
    Task<IMessagePayload?> DeserializeAsync(Stream stream);
}

/// <summary>
/// Generic version for type safety
/// </summary>
public interface IPayloadTypeSerializer<TMessagePayload>
    : IPayloadTypeSerializer where TMessagePayload : IMessagePayload
{
    new Task<TMessagePayload?> DeserializeAsync(Stream stream);
}