namespace NLightning.Infrastructure.Serialization.Interfaces;

using Domain.Protocol.Models;

public interface ITlvStreamSerializer
{
    Task SerializeAsync(TlvStream baseTlv, Stream stream);
    Task<TlvStream?> DeserializeAsync(Stream stream);
}