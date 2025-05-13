namespace NLightning.Infrastructure.Serialization.Interfaces;

using Domain.Protocol.Models;

public interface ITlvSerializer
{
    Task SerializeAsync(BaseTlv baseTlv, Stream stream);
    Task<TlvType> DeserializeAsync<TlvType>(Stream stream) where TlvType : BaseTlv;
}

public interface ITlvSerializer<TlvType> where TlvType : BaseTlv
{
    Task<TlvType> DeserializeAsync(Stream stream);
}