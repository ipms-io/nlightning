namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Models;
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
        var nextFundingTlv = (NextFundingTlv)baseTlv;
        
        if (nextFundingTlv.Type != TlvConstants.NEXT_FUNDING)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (nextFundingTlv.Length != 32)
        {
            throw new InvalidCastException("Invalid length");
        }

        nextFundingTlv.Value = nextFundingTlv.NextFundingTxId;
        
        return nextFundingTlv;
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