namespace NLightning.Common.TLVs;

using Constants;
using Types;

/// <summary>
/// Required Confirmed Inputs TLV.
/// </summary>
/// <remarks>
/// The required confirmed inputs TLV is used in the TxInitRbfMessage to communicate if confirmed inputs are required.
/// </remarks>
public class RequireConfirmedInputsTlv() : Tlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS)
{
    public static RequireConfirmedInputsTlv? FromTlv(Tlv tlv)
    {
        if (tlv.Type != TlvConstants.REQUIRE_CONFIRMED_INPUTS)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (tlv.Length != 0) // long (64 bits) is 8 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        return new RequireConfirmedInputsTlv();
    }
}