using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Tlv;

public class NextFundingTlvConverter : ITlvConverter<NextFundingTlv>
{
    public BaseTlv ConvertToBase(NextFundingTlv tlv)
    {
        return tlv;
    }

    public NextFundingTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.NextFunding)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length != 32)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new NextFundingTlv(baseTlv.Value);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as NextFundingTlv
                          ?? throw new InvalidCastException(
                                 $"Error converting BaseTlv to {nameof(NextFundingTlv)}"));
    }
}