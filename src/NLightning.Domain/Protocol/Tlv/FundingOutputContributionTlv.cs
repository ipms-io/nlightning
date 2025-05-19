namespace NLightning.Domain.Protocol.Tlv;

using Constants;
using Money;

/// <summary>
/// Funding Output Contribution TLV.
/// </summary>
/// <remarks>
/// The funding output contribution TLV is used in the TxInitRbfMessage to communicate the funding output contribution in satoshis.
/// </remarks>
public class FundingOutputContributionTlv : BaseTlv
{
    /// <summary>
    /// The amount being contributed in satoshis
    /// </summary>
    public LightningMoney Amount { get; }

    public FundingOutputContributionTlv(LightningMoney amount) : base(TlvConstants.FUNDING_OUTPUT_CONTRIBUTION)
    {
        Amount = amount;
        Length = sizeof(ulong);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Length, Amount.GetHashCode());
    }
    
    public override bool Equals(object? obj)
    {
        return obj is FundingOutputContributionTlv fundingOutputContributionTlv && Equals(fundingOutputContributionTlv);
    }

    private bool Equals(FundingOutputContributionTlv other)
    {
        return Type.Equals(other.Type) && Length.Equals(other.Length) && Amount.Equals(other.Amount);
    }
}