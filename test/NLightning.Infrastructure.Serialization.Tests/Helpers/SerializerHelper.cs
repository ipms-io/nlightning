namespace NLightning.Infrastructure.Serialization.Tests.Helpers;

using Protocol.Factories;
using Serialization.Factories;
using Serialization.Node;
using Serialization.Tlv;

public static class SerializerHelper
{
    public static readonly ValueObjectSerializerFactory ValueObjectSerializerFactory;
    public static readonly PayloadSerializerFactory PayloadSerializerFactory;
    public static readonly TlvConverterFactory TlvConverterFactory;
    public static readonly TlvStreamSerializer TlvStreamSerializer;
    public static readonly TlvSerializer TlvSerializer;

    static SerializerHelper()
    {
        ValueObjectSerializerFactory = new ValueObjectSerializerFactory();
        PayloadSerializerFactory =
            new PayloadSerializerFactory(new FeatureSetSerializer(), ValueObjectSerializerFactory);
        TlvConverterFactory = new TlvConverterFactory();
        TlvStreamSerializer =
            new TlvStreamSerializer(TlvConverterFactory, new TlvSerializer(ValueObjectSerializerFactory));
        TlvSerializer = new TlvSerializer(ValueObjectSerializerFactory);
    }
}