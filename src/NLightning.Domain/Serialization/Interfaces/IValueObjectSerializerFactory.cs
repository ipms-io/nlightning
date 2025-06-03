using NLightning.Domain.Interfaces;

namespace NLightning.Domain.Serialization.Interfaces;

public interface IValueObjectSerializerFactory
{
    IValueObjectTypeSerializer<TValueObjectType>? GetSerializer<TValueObjectType>()
        where TValueObjectType : IValueObject;
}