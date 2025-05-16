namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Enums;
using Domain.Money;
using Domain.Protocol.Constants;
using Domain.Protocol.Models;
using Domain.Protocol.Tlv;
using Domain.Protocol.Tlv.Converters;
using Infrastructure.Converters;

public class FeeRangeTlvConverter : ITlvConverter<FeeRangeTlv>
{
    public BaseTlv ConvertToBase(FeeRangeTlv tlv)
    {
        var minSatsBytes = EndianBitConverter.GetBytesBigEndian(tlv.MinFeeAmount.Satoshi);
        var maxSatsBytes = EndianBitConverter.GetBytesBigEndian(tlv.MaxFeeAmount.Satoshi);
        
        minSatsBytes.CopyTo(tlv.Value, 0);
        maxSatsBytes.CopyTo(tlv.Value, sizeof(ulong));
        
        return tlv;
    }

    public FeeRangeTlv ConvertFromBase(BaseTlv baseTlv)
    {
        var feeRangeTlv = (FeeRangeTlv)baseTlv;
        
        if (feeRangeTlv.Type != TlvConstants.FEE_RANGE)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length != sizeof(ulong) * 2) // 2 long (128 bits) is 16 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        feeRangeTlv.MinFeeAmount = LightningMoney
            .FromUnit(EndianBitConverter.ToUInt64BigEndian(baseTlv.Value[..sizeof(ulong)]), LightningMoneyUnit.Satoshi);
        feeRangeTlv.MaxFeeAmount = LightningMoney
            .FromUnit(EndianBitConverter.ToUInt64BigEndian(baseTlv.Value[sizeof(ulong)..]), LightningMoneyUnit.Satoshi);

        return feeRangeTlv;
    }

    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as FeeRangeTlv 
                             ?? throw new InvalidCastException($"Error converting BaseTlv to {nameof(FeeRangeTlv)}"));
    }
}