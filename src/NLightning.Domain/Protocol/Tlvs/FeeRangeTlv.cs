using NLightning.Domain.Enums;
using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Serialization;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Tlvs;

/// <summary>
/// Fee Range TLV.
/// </summary>
/// <remarks>
/// The fee range TLV is used in the ClosingSignedMessage to set our accepted fee range.
/// </remarks>
public class FeeRangeTlv : Tlv
{
    private static IEndianConverter? s_endianConverter;
    private static IEndianConverter _endianConverter => 
        s_endianConverter ?? throw new InvalidOperationException("EndianConverter not initialized");
    
    public static void SetEndianConverter(IEndianConverter converter) => s_endianConverter = converter;
    
    /// <summary>
    /// The minimum acceptable fee in satoshis
    /// </summary>
    public LightningMoney MinFeeAmount { get; private set; }

    /// <summary>
    /// The maximum acceptable fee in satoshis
    /// </summary>
    public LightningMoney MaxFeeAmount { get; private set; }

    public FeeRangeTlv(LightningMoney minFeeAmount, LightningMoney maxFeeAmount) : base(TlvConstants.FEE_RANGE)
    {
        MinFeeAmount = minFeeAmount;
        MaxFeeAmount = maxFeeAmount;

        var minSatsBytes = _endianConverter.GetBytesBigEndian(MinFeeAmount.Satoshi);
        var maxSatsBytes = _endianConverter.GetBytesBigEndian(MaxFeeAmount.Satoshi);

        Length = sizeof(ulong) * 2;
        Value = new byte[Length];
        minSatsBytes.CopyTo(Value, 0);
        maxSatsBytes.CopyTo(Value, sizeof(ulong));
    }

    /// <summary>
    /// Cast FeeRangeTlv from a Tlv.
    /// </summary>
    /// <param name="tlv">The tlv to cast from.</param>
    /// <returns>The cast FeeRangeTlv.</returns>
    /// <exception cref="InvalidCastException">Error casting FeeRangeTlv</exception>
    public static FeeRangeTlv FromTlv(Tlv tlv)
    {
        if (tlv.Type != TlvConstants.FEE_RANGE)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (tlv.Length != sizeof(ulong) * 2) // 2 long (128 bits) is 16 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        var minFeeSatoshis = LightningMoney.FromUnit(_endianConverter.ToUInt64BigEndian(tlv.Value[..sizeof(ulong)]),
                                                     LightningMoneyUnit.SATOSHI);
        var maxFeeSatoshis = LightningMoney.FromUnit(_endianConverter.ToUInt64BigEndian(tlv.Value[sizeof(ulong)..]),
                                                     LightningMoneyUnit.SATOSHI);

        return new FeeRangeTlv(minFeeSatoshis, maxFeeSatoshis);
    }
}