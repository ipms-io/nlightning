namespace NLightning.Domain.Protocol.Tlv;

using Constants;

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
}