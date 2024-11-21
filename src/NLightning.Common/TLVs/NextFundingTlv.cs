namespace NLightning.Common.TLVs;

using Constants;
using Types;

/// <summary>
/// Blinded Path TLV.
/// </summary>
/// <remarks>
/// The blinded path TLV is used in the UpdateAddHtlcMessage to communicate the blinded path key.
/// </remarks>
public class NextFundingTlv : Tlv
{
    /// <summary>
    /// The blinded path key
    /// </summary>
    public byte[] NextFundingTxId { get; }

    public NextFundingTlv(byte[] nextFundingTxId) : base(TlvConstants.NEXT_FUNDING)
    {
        NextFundingTxId = nextFundingTxId;

        Value = NextFundingTxId;
        Length = Value.Length;
    }

    /// <summary>
    /// Cast NextFundingTlv from a Tlv.
    /// </summary>
    /// <param name="tlv">The tlv to cast from.</param>
    /// <returns>The cast NextFundingTlv.</returns>
    /// <exception cref="InvalidCastException">Error casting NextFundingTlv</exception>
    public static NextFundingTlv FromTlv(Tlv tlv)
    {
        if (tlv.Type != TlvConstants.NEXT_FUNDING)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (tlv.Length != 32)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new NextFundingTlv(tlv.Value);
    }
}