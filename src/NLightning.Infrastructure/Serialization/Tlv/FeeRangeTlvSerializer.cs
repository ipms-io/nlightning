namespace NLightning.Infrastructure.Serialization.Tlv;

using Domain.Enums;
using Domain.Money;
using Domain.Protocol.Constants;
using Domain.Protocol.Models;
using Domain.Protocol.Tlvs;
using Interfaces;

public class FeeRangeTlvSerializer : TlvSerializer, ITlvSerializer<FeeRangeTlv>
{
    public FeeRangeTlvSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
        : base(valueObjectSerializerFactory)
    {
    }

    public async Task<FeeRangeTlv> DeserializeAsync(Stream stream)
    {
        var baseTlv = await base.DeserializeAsync<BaseTlv>(stream);
        if (baseTlv.Type != TlvConstants.FeeRange)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length != sizeof(ulong) * 2) // 2 long (128 bits) is 16 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        var minFeeSatoshis = LightningMoney
            .FromUnit(EndianBitConverter.ToUInt64BigEndian(baseTlv.Value[..sizeof(ulong)]), LightningMoneyUnit.Satoshi);
        var maxFeeSatoshis = LightningMoney
            .FromUnit(EndianBitConverter.ToUInt64BigEndian(baseTlv.Value[sizeof(ulong)..]), LightningMoneyUnit.Satoshi);

        return new FeeRangeTlv(minFeeSatoshis, maxFeeSatoshis);
    }

    public async Task SerializeAsync(FeeRangeTlv feeRangeTlv, Stream stream)
    {
        var minSatsBytes = EndianBitConverter.GetBytesBigEndian(feeRangeTlv.MinFeeAmount.Satoshi);
        var maxSatsBytes = EndianBitConverter.GetBytesBigEndian(feeRangeTlv.MaxFeeAmount.Satoshi);
        
        minSatsBytes.CopyTo(feeRangeTlv.Value, 0);
        maxSatsBytes.CopyTo(feeRangeTlv.Value, sizeof(ulong));
        
        await base.SerializeAsync(feeRangeTlv, stream);
    }
}