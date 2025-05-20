namespace NLightning.Domain.Serialization.Factories;

using Domain.ValueObjects.Interfaces;
using ValueObjects;

public interface IValueObjectSerializerFactory
{
    IValueObjectTypeSerializer<TValueObjectType>? GetSerializer<TValueObjectType>()
        where TValueObjectType : IValueObject;
}