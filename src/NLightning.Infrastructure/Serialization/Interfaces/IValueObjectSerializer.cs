using NLightning.Domain.ValueObjects.Interfaces;

namespace NLightning.Infrastructure.Serialization.Interfaces;

/// <summary>
/// Interface for serializers that handle specific value object types
/// </summary>
public interface IValueObjectSerializer
{
    Task SerializeAsync(IValueObject valueObject, Stream stream);
    Task<IValueObject> DeserializeAsync(Stream stream);
}

/// <summary>
/// Generic version for type safety
/// </summary>
public interface IValueObjectSerializer<TValueObject>: IValueObjectSerializer where TValueObject : IValueObject
{
    new Task<TValueObject> DeserializeAsync(Stream stream);
}