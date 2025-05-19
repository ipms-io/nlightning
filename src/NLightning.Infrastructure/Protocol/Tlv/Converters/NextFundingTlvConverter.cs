namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Tlv;
using Domain.Protocol.Tlv.Converters;

public class NextFundingTlvConverter : ITlvConverter<NextFundingTlv>
{
    public BaseTlv ConvertToBase(NextFundingTlv tlv)
    {
        return tlv;
    }

    public NextFundingTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.NEXT_FUNDING)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length != 32)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new NextFundingTlv(baseTlv.Value);
    }

    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as NextFundingTlv
                             ?? throw new InvalidCastException(
                                 $"Error converting BaseTlv to {nameof(NextFundingTlv)}"));
    }
}