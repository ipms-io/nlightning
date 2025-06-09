using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Enums;
using Domain.Money;
using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Tlv;
using Infrastructure.Converters;

public class FeeRangeTlvConverter : ITlvConverter<FeeRangeTlv>
{
    public BaseTlv ConvertToBase(FeeRangeTlv tlv)
    {
        var tlvValue = new byte[sizeof(ulong) * 2];
        EndianBitConverter.GetBytesBigEndian(tlv.MinFeeAmount.Satoshi).CopyTo(tlvValue, 0);
        EndianBitConverter.GetBytesBigEndian(tlv.MaxFeeAmount.Satoshi).CopyTo(tlvValue, sizeof(ulong));

        return new BaseTlv(tlv.Type, tlvValue);
    }

    public FeeRangeTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.FeeRange)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length != sizeof(ulong) * 2) // 2 long (128 bits) is 16 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        var minFeeAmount = LightningMoney
           .FromUnit(EndianBitConverter.ToUInt64BigEndian(baseTlv.Value[..sizeof(ulong)]), LightningMoneyUnit.Satoshi);
        var maxFeeAmount = LightningMoney
           .FromUnit(EndianBitConverter.ToUInt64BigEndian(baseTlv.Value[sizeof(ulong)..]), LightningMoneyUnit.Satoshi);

        return new FeeRangeTlv(minFeeAmount, maxFeeAmount);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as FeeRangeTlv
                          ?? throw new InvalidCastException($"Error converting BaseTlv to {nameof(FeeRangeTlv)}"));
    }
}