using NLightning.Domain.ValueObjects.Interfaces;

namespace NLightning.Infrastructure.Serialization.Interfaces;

public interface IValueObjectSerializerFactory
{
    Task SerializeAsync(IValueObject valueObject, Stream stream);
    Task<TValueObject> DeserializeAsync<TValueObject>(Stream stream) where TValueObject : IValueObject;
}