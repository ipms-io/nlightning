using NLightning.Domain.Enums;
using NLightning.Domain.Money;
using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Models;
using NLightning.Domain.Serialization;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Tlvs;

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
    public LightningMoney MinFeeAmount { get; private set; }

    /// <summary>
    /// The maximum acceptable fee in satoshis
    /// </summary>
    public LightningMoney MaxFeeAmount { get; private set; }

    public FeeRangeTlv(LightningMoney minFeeAmount, LightningMoney maxFeeAmount) : base(TlvConstants.FeeRange)
    {
        MinFeeAmount = minFeeAmount;
        MaxFeeAmount = maxFeeAmount;

        Length = sizeof(ulong) * 2;
        Value = new byte[Length];
    }
}