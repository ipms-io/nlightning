using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Tlv;

using Domain.Protocol.Factories;
using Domain.Protocol.Models;
using Domain.Protocol.Tlv;
using Domain.Serialization.Tlv;
using Interfaces;

public class TlvStreamSerializer : ITlvStreamSerializer
{
    private readonly ITlvSerializer _tlvSerializer;
    private readonly ITlvConverterFactory _tlvConverterFactory;

    public TlvStreamSerializer(ITlvConverterFactory tlvConverterFactory, ITlvSerializer tlvSerializer)
    {
        _tlvConverterFactory = tlvConverterFactory;
        _tlvSerializer = tlvSerializer;
    }
    
    public async Task SerializeAsync(TlvStream? tlvStream, Stream stream)
    {
        if (tlvStream is null)
            return;
        
        foreach (var tlv in tlvStream.GetTlvs())
        {
            var baseTlv = tlv switch
            {
                BlindedPathTlv upfrontShutdownScriptTlv => _tlvConverterFactory
                    .GetConverter<BlindedPathTlv>()?.ConvertToBase(upfrontShutdownScriptTlv),
                ChannelTypeTlv upfrontShutdownScriptTlv => _tlvConverterFactory
                    .GetConverter<ChannelTypeTlv>()?.ConvertToBase(upfrontShutdownScriptTlv),
                FeeRangeTlv upfrontShutdownScriptTlv => _tlvConverterFactory
                    .GetConverter<FeeRangeTlv>()?.ConvertToBase(upfrontShutdownScriptTlv),
                FundingOutputContributionTlv upfrontShutdownScriptTlv => _tlvConverterFactory
                    .GetConverter<FundingOutputContributionTlv>()?.ConvertToBase(upfrontShutdownScriptTlv),
                NetworksTlv upfrontShutdownScriptTlv => _tlvConverterFactory
                    .GetConverter<NetworksTlv>()?.ConvertToBase(upfrontShutdownScriptTlv),
                NextFundingTlv upfrontShutdownScriptTlv => _tlvConverterFactory
                    .GetConverter<NextFundingTlv>()?.ConvertToBase(upfrontShutdownScriptTlv),
                RequireConfirmedInputsTlv upfrontShutdownScriptTlv => _tlvConverterFactory
                    .GetConverter<RequireConfirmedInputsTlv>()?.ConvertToBase(upfrontShutdownScriptTlv),
                ShortChannelIdTlv upfrontShutdownScriptTlv => _tlvConverterFactory
                    .GetConverter<ShortChannelIdTlv>()?.ConvertToBase(upfrontShutdownScriptTlv),
                UpfrontShutdownScriptTlv upfrontShutdownScriptTlv => _tlvConverterFactory
                    .GetConverter<UpfrontShutdownScriptTlv>()?.ConvertToBase(upfrontShutdownScriptTlv),
                _ => null
            };
            
            if (baseTlv is null)
                throw new SerializationException($"No converter found for tlv type {tlv.GetType().Name}");
            
            await _tlvSerializer.SerializeAsync(baseTlv, stream);
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