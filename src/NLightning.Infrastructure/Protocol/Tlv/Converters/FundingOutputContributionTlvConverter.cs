using System.Diagnostics.CodeAnalysis;
using NLightning.Domain.Money;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Tlv;
using Infrastructure.Converters;

public class FundingOutputContributionTlvConverter : ITlvConverter<FundingOutputContributionTlv>
{
    public BaseTlv ConvertToBase(FundingOutputContributionTlv tlv)
    {
        return new BaseTlv(tlv.Type, EndianBitConverter.GetBytesBigEndian(tlv.Amount.Satoshi));
    }

    public FundingOutputContributionTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.FundingOutputContribution)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length != 8) // long (64 bits) is 8 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        var amount = LightningMoney.Satoshis(EndianBitConverter.ToInt64BigEndian(baseTlv.Value));

        return new FundingOutputContributionTlv(amount);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as FundingOutputContributionTlv
                          ?? throw new InvalidCastException(
                                 $"Error casting BaseTlv to {nameof(FundingOutputContributionTlv)}"));
    }
}