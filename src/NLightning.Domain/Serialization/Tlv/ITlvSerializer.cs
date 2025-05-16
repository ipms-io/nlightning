namespace NLightning.Domain.Serialization.Tlv;

using Protocol.Models;

public interface ITlvSerializer
{
    Task SerializeAsync(BaseTlv baseTlv, Stream stream);
    Task<BaseTlv?> DeserializeAsync(Stream stream);
}