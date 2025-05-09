namespace NLightning.Application.Interfaces.Serialization;

using Domain.Protocol.Interfaces;

/// <summary>
/// Interface for serializers that handle specific message types
/// </summary>
public interface IMessageTypeSerializer
{
    Task SerializeAsync(IMessage message, Stream stream);
    Task<IMessage> DeserializeAsync(Stream stream);
}

/// <summary>
/// Generic version for type safety
/// </summary>
public interface IMessageTypeSerializer<TMessage> : IMessageTypeSerializer where TMessage : IMessage
{
    new Task<TMessage> DeserializeAsync(Stream stream);
}