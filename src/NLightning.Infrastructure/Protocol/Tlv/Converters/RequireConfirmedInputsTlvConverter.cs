namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Models;
using Domain.Protocol.Tlv;
using Domain.Protocol.Tlv.Converters;

public class RequireConfirmedInputsTlvConverter : ITlvConverter<RequireConfirmedInputsTlv>
{
    public BaseTlv ConvertToBase(RequireConfirmedInputsTlv tlv)
    {
        return tlv;
    }

    public RequireConfirmedInputsTlv ConvertFromBase(BaseTlv baseTlv)
    {
        var requireConfirmedInputsTlv = (RequireConfirmedInputsTlv)baseTlv;

        if (requireConfirmedInputsTlv.Type != TlvConstants.REQUIRE_CONFIRMED_INPUTS)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (requireConfirmedInputsTlv.Length != 0)
        {
            throw new InvalidCastException("Invalid length");
        }
        
        return requireConfirmedInputsTlv;
    }

    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as RequireConfirmedInputsTlv 
                             ?? throw new InvalidCastException(
                                 $"Error converting BaseTlv to {nameof(RequireConfirmedInputsTlv)}"));
    }
}