using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Tlv;

public class ShortChannelIdTlvConverter : ITlvConverter<ShortChannelIdTlv>
{
    public BaseTlv ConvertToBase(ShortChannelIdTlv tlv)
    {
        tlv.Value = tlv.ShortChannelId;

        return tlv;
    }

    public ShortChannelIdTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.ShortChannelId)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new ShortChannelIdTlv(baseTlv.Value);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as ShortChannelIdTlv
                          ?? throw new InvalidCastException(
                                 $"Error converting BaseTlv to {nameof(ShortChannelIdTlv)}"));
    }
}