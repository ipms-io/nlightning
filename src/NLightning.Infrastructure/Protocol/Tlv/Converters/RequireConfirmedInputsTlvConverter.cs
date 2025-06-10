using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Tlv;

public class RequireConfirmedInputsTlvConverter : ITlvConverter<RequireConfirmedInputsTlv>
{
    public BaseTlv ConvertToBase(RequireConfirmedInputsTlv tlv)
    {
        return tlv;
    }

    public RequireConfirmedInputsTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.RequireConfirmedInputs)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length != 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new RequireConfirmedInputsTlv();
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as RequireConfirmedInputsTlv
                          ?? throw new InvalidCastException(
                                 $"Error converting BaseTlv to {nameof(RequireConfirmedInputsTlv)}"));
    }
}