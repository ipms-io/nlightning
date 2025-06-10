namespace NLightning.Domain.Serialization.Interfaces;

using Domain.Interfaces;

/// <summary>
/// Interface for serializers that handle specific value object types
/// </summary>
public interface IValueObjectTypeSerializer
{
    Task SerializeAsync(IValueObject valueObject, Stream stream);
    Task<IValueObject> DeserializeAsync(Stream stream);
}

/// <summary>
/// Generic version for type safety
/// </summary>
public interface IValueObjectTypeSerializer<TValueObject> : IValueObjectTypeSerializer where TValueObject : IValueObject
{
    new Task<TValueObject> DeserializeAsync(Stream stream);
}