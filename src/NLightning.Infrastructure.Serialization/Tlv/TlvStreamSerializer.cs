using System.Runtime.Serialization;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Tlv;

using Domain.Protocol.Factories;
using Domain.Protocol.Models;
using Domain.Protocol.Tlv;
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
                BlindedPathTlv blindedPathTlv => _tlvConverterFactory
                                                .GetConverter<BlindedPathTlv>()?.ConvertToBase(blindedPathTlv),
                ChannelTypeTlv channelTypeTlv => _tlvConverterFactory
                                                .GetConverter<ChannelTypeTlv>()?.ConvertToBase(channelTypeTlv),
                FeeRangeTlv feeRangeTlv => _tlvConverterFactory
                                          .GetConverter<FeeRangeTlv>()?.ConvertToBase(feeRangeTlv),
                FundingOutputContributionTlv fundingOutputContributionTlv => _tlvConverterFactory
                                                                            .GetConverter<
                                                                                 FundingOutputContributionTlv>()
                                                                           ?.ConvertToBase(
                                                                                 fundingOutputContributionTlv),
                NetworksTlv networksTlv => _tlvConverterFactory
                                          .GetConverter<NetworksTlv>()?.ConvertToBase(networksTlv),
                NextFundingTlv nextFundingTlv => _tlvConverterFactory
                                                .GetConverter<NextFundingTlv>()?.ConvertToBase(nextFundingTlv),
                RequireConfirmedInputsTlv requireConfirmedInputsTlv => _tlvConverterFactory
                                                                      .GetConverter<RequireConfirmedInputsTlv>()
                                                                     ?.ConvertToBase(requireConfirmedInputsTlv),
                ShortChannelIdTlv shortChannelIdTlv => _tlvConverterFactory
                                                      .GetConverter<ShortChannelIdTlv>()
                                                     ?.ConvertToBase(shortChannelIdTlv),
                UpfrontShutdownScriptTlv upfrontShutdownScriptTlv => _tlvConverterFactory
                                                                    .GetConverter<UpfrontShutdownScriptTlv>()
                                                                   ?.ConvertToBase(upfrontShutdownScriptTlv),
                _ => null
            } ?? throw new SerializationException($"No converter found for tlv type {tlv.GetType().Name}");
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

            return tlvStream.Any() ? tlvStream : null;
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TLVStream", e);
        }
    }
}