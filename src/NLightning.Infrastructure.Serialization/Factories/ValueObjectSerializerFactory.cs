using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Interfaces;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Factories;

using Domain.Channels.ValueObjects;
using Domain.Protocol.ValueObjects;
using ValueObjects;

public class ValueObjectSerializerFactory : IValueObjectSerializerFactory
{
    private readonly Dictionary<Type, IValueObjectTypeSerializer> _serializers = new();

    public ValueObjectSerializerFactory()
    {
        RegisterSerializers();
    }

    public IValueObjectTypeSerializer<TValueObjectType>? GetSerializer<TValueObjectType>()
        where TValueObjectType : IValueObject
    {
        return _serializers.GetValueOrDefault(typeof(TValueObjectType)) as IValueObjectTypeSerializer<TValueObjectType>;
    }

    private void RegisterSerializers()
    {
        _serializers.Add(typeof(BigSize), new BigSizeTypeSerializer());
        _serializers.Add(typeof(ChainHash), new ChainHashTypeSerializer());
        _serializers.Add(typeof(ChannelFlags), new ChannelFlagTypeSerializer());
        _serializers.Add(typeof(ChannelId), new ChannelIdTypeSerializer());
        _serializers.Add(typeof(ShortChannelId), new ShortChannelIdTypeSerializer());
        _serializers.Add(typeof(Witness), new WitnessTypeSerializer());
    }
}