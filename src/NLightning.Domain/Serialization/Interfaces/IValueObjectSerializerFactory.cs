namespace NLightning.Domain.Serialization.Interfaces;

using Domain.Interfaces;

public interface IValueObjectSerializerFactory
{
    IValueObjectTypeSerializer<TValueObjectType>? GetSerializer<TValueObjectType>()
        where TValueObjectType : IValueObject;
}