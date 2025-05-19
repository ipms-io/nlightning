namespace NLightning.Domain.Protocol.Tlv;

using Constants;
using Money;

/// <summary>
/// Fee Range TLV.
/// </summary>
/// <remarks>
/// The fee range TLV is used in the ClosingSignedMessage to set our accepted fee range.
/// </remarks>
public class FeeRangeTlv : BaseTlv
{
    /// <summary>
    /// The minimum acceptable fee in satoshis
    /// </summary>
    public LightningMoney MinFeeAmount { get; }

    /// <summary>
    /// The maximum acceptable fee in satoshis
    /// </summary>
    public LightningMoney MaxFeeAmount { get; }

    public FeeRangeTlv(LightningMoney minFeeAmount, LightningMoney maxFeeAmount) : base(TlvConstants.FEE_RANGE)
    {
        MinFeeAmount = minFeeAmount;
        MaxFeeAmount = maxFeeAmount;

        Length = sizeof(ulong) * 2;
        Value = new byte[Length];
    }
}