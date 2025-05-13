using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Factories;

using Domain.ValueObjects;
using Domain.ValueObjects.Interfaces;
using Interfaces;
using ValueObjects;

public class ValueObjectSerializerFactory : IValueObjectSerializerFactory
{
    private readonly Dictionary<Type, IValueObjectSerializer> _serializers = new();

    public ValueObjectSerializerFactory()
    {
        RegisterSerializers();
    }

    public Task SerializeAsync(IValueObject valueObject, Stream stream)
    {
        if (_serializers.TryGetValue(valueObject.GetType(), out var serializer))
        {
            return serializer.SerializeAsync(valueObject, stream);
        }
        
        throw new SerializationException($"No serializer found for value object type {valueObject.GetType()}");
    }

    public Task<TValueObject> DeserializeAsync<TValueObject>(Stream stream) where TValueObject : IValueObject
    {
        if (_serializers.TryGetValue(typeof(TValueObject), out var serializer))
        {
            return serializer.DeserializeAsync(stream) as Task<TValueObject>
                   ?? throw new SerializationException($"Deserialized value object is not of type {typeof(TValueObject)}");
        }
        
        throw new SerializationException($"No serializer found for value object type {typeof(TValueObject)}");
    }

    private void RegisterSerializers()
    {
        _serializers.Add(typeof(BigSize), new BigSizeSerializer());
        _serializers.Add(typeof(ChainHash), new ChainHashSerializer());
        _serializers.Add(typeof(ChannelFlags), new ChannelFlagSerializer());
        _serializers.Add(typeof(ChannelId), new ChannelIdSerializer());
        _serializers.Add(typeof(ShortChannelId), new ShortChannelIdSerializer());
        _serializers.Add(typeof(Witness), new WitnessSerializer());
    }
}