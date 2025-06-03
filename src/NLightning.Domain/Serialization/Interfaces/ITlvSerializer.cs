using NLightning.Domain.Protocol.Tlv;

namespace NLightning.Domain.Serialization.Interfaces;

public interface ITlvSerializer
{
    Task SerializeAsync(BaseTlv baseTlv, Stream stream);
    Task<BaseTlv?> DeserializeAsync(Stream stream);
}