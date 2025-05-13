using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Models;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Tlvs;

/// <summary>
/// Blinded Path TLV.
/// </summary>
/// <remarks>
/// The blinded path TLV is used in the UpdateAddHtlcMessage to communicate the blinded path key.
/// </remarks>
public class NextFundingTlv : BaseTlv
{
    /// <summary>
    /// The blinded path key
    /// </summary>
    public byte[] NextFundingTxId { get; }

    public NextFundingTlv(byte[] nextFundingTxId) : base(TlvConstants.NextFunding)
    {
        NextFundingTxId = nextFundingTxId;

        Value = NextFundingTxId;
        Length = Value.Length;
    }

    /// <summary>
    /// Cast NextFundingTlv from a BaseTlv.
    /// </summary>
    /// <param name="baseTlv">The baseTlv to cast from.</param>
    /// <returns>The cast NextFundingTlv.</returns>
    /// <exception cref="InvalidCastException">Error casting NextFundingTlv</exception>
    public static NextFundingTlv FromTlv(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.NextFunding)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length != 32)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new NextFundingTlv(baseTlv.Value);
    }
}