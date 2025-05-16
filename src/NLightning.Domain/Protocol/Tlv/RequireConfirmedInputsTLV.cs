namespace NLightning.Domain.Protocol.Tlv;

using Constants;
using Models;

/// <summary>
/// Required Confirmed Inputs TLV.
/// </summary>
/// <remarks>
/// The required confirmed inputs TLV is used in the TxInitRbfMessage to communicate if confirmed inputs are required.
/// </remarks>
public class RequireConfirmedInputsTlv() : BaseTlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS);