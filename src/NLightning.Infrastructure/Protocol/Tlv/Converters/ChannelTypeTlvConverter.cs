using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Tlv;

public class ChannelTypeTlvConverter : ITlvConverter<ChannelTypeTlv>
{
    public BaseTlv ConvertToBase(ChannelTypeTlv tlv)
    {
        tlv.Value = tlv.ChannelType;

        return tlv;
    }

    public ChannelTypeTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.ChannelType)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new ChannelTypeTlv(baseTlv.Value);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as ChannelTypeTlv
                          ?? throw new InvalidCastException($"Error converting BaseTlv to {nameof(ChannelTypeTlv)}"));
    }
}