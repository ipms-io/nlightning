using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Infrastructure.Serialization.Interfaces;

/// <summary>
/// Interface for serializers that handle specific message types
/// </summary>
public interface IMessageSerializer
{
    Task SerializeAsync(IMessage message, Stream stream);
    Task<IMessage> DeserializeAsync(Stream stream);
}

/// <summary>
/// Generic version for type safety
/// </summary>
public interface IMessageSerializer<TMessage> : IMessageSerializer where TMessage : IMessage
{
    new Task<TMessage> DeserializeAsync(Stream stream);
}