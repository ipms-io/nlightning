using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Tlv;

using Domain.Protocol.Models;
using Domain.Serialization.Tlv;
using Interfaces;

public class TlvStreamSerializer : ITlvStreamSerializer
{
    private readonly ITlvSerializer _tlvSerializer;

    public TlvStreamSerializer(ITlvSerializer tlvSerializer)
    {
        _tlvSerializer = tlvSerializer;
    }
    
    public async Task SerializeAsync(TlvStream tlvStream, Stream stream)
    {
        foreach (var tlv in tlvStream.GetTlvs())
        {
            await _tlvSerializer.SerializeAsync(tlv, stream);
        }
    }

    public async Task<TlvStream?> DeserializeAsync(Stream stream)
    {
        if (stream.Position == stream.Length)
            return null;

        try
        {
            var tlvStream = new TlvStream();

            while (stream.Position != stream.Length)
            {
                var tlv = await _tlvSerializer.DeserializeAsync(stream);
                if (tlv is not null)
                    tlvStream.Add(tlv);
            }

            return tlvStream;
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TLVStream", e);
        }
    }
}