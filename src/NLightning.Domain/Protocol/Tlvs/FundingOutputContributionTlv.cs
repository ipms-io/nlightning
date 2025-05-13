namespace NLightning.Domain.Protocol.Tlvs;

using Constants;
using Models;

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
    public long Satoshis { get; }

    public FundingOutputContributionTlv(long satoshis) : base(TlvConstants.FundingOutputContribution)
    {
        Satoshis = satoshis;
    }
}