namespace NLightning.Infrastructure.Serialization.Factories;

using Domain.Protocol.Models;
using Domain.Protocol.Tlvs;
using Interfaces;
using Tlv;

public class TlvSerializerFactory : ITlvSerializerFactory
{
    private readonly Dictionary<Type, ITlvSerializer> _serializers = new();
    private readonly TlvSerializer _defaultTlvSerializer;

    public TlvSerializerFactory(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _defaultTlvSerializer = new TlvSerializer(valueObjectSerializerFactory);
        RegisterSerializers(valueObjectSerializerFactory);
    }

    public Task SerializeAsync(BaseTlv tlv, Stream stream)
    {
        if (_serializers.TryGetValue(tlv.GetType(), out var serializer))
        {
            return serializer.SerializeAsync(tlv, stream);
        }
        
        return _defaultTlvSerializer.SerializeAsync(tlv, stream);
    }

    public Task<TTlv> DeserializeAsync<TTlv>(Stream stream) where TTlv : BaseTlv
    {
        if (_serializers.TryGetValue(typeof(TTlv), out var serializer))
        {
            return serializer.DeserializeAsync<TTlv>(stream);
        }
        
        return _defaultTlvSerializer.DeserializeAsync<TTlv>(stream);
    }

    private void RegisterSerializers(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _serializers.Add(typeof(FundingOutputContributionTlv),
                         new FundingOutputContributionTlvSerializer(valueObjectSerializerFactory));
        _serializers.Add(typeof(FeeRangeTlv), new FeeRangeTlvSerializer(valueObjectSerializerFactory));
    }
}