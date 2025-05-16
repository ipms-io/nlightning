namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Models;
using Domain.Protocol.Tlv;
using Domain.Protocol.Tlv.Converters;
using Infrastructure.Converters;

public class FundingOutputContributionConverter : ITlvConverter<FundingOutputContributionTlv>
{
    public BaseTlv ConvertToBase(FundingOutputContributionTlv tlv)
    {
        tlv.Value = EndianBitConverter.GetBytesBigEndian(tlv.Satoshis);
        
        return tlv;
    }

    public FundingOutputContributionTlv ConvertFromBase(BaseTlv baseTlv)
    {
        var fundingTlv = (FundingOutputContributionTlv)baseTlv;
        
        if (fundingTlv.Type != TlvConstants.FUNDING_OUTPUT_CONTRIBUTION)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (fundingTlv.Length != 8) // long (64 bits) is 8 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        fundingTlv.Satoshis = EndianBitConverter.ToInt64BigEndian(baseTlv.Value);

        return fundingTlv;
    }

    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as FundingOutputContributionTlv 
                              ?? throw new InvalidCastException(
                                  $"Error casting BaseTlv to {nameof(FundingOutputContributionTlv)}"));
    }
}