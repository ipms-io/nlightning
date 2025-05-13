using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Models;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Tlvs;

/// <summary>
/// Required Confirmed Inputs TLV.
/// </summary>
/// <remarks>
/// The required confirmed inputs TLV is used in the TxInitRbfMessage to communicate if confirmed inputs are required.
/// </remarks>
public class RequireConfirmedInputsTlv() : BaseTlv(TlvConstants.RequireConfirmedInputs)
{
    public static RequireConfirmedInputsTlv FromTlv(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.RequireConfirmedInputs)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length != 0) // long (64 bits) is 8 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        return new RequireConfirmedInputsTlv();
    }
}