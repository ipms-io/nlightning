namespace NLightning.Infrastructure.Serialization.Interfaces;

using Domain.Protocol.Models;

public interface ITlvSerializerFactory
{
    Task SerializeAsync(BaseTlv tlv, Stream stream);
    Task<TTlv> DeserializeAsync<TTlv>(Stream stream) where TTlv : BaseTlv;
}