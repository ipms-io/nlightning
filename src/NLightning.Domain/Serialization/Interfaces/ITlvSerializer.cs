namespace NLightning.Domain.Serialization.Interfaces;

using Protocol.Tlv;

public interface ITlvSerializer
{
    Task SerializeAsync(BaseTlv baseTlv, Stream stream);
    Task<BaseTlv?> DeserializeAsync(Stream stream);
}