namespace NLightning.Common.TLVs;

using Constants;
using Types;

/// <summary>
/// Required Confirmed Inputs TLV.
/// </summary>
/// <remarks>
/// The required confirmed inputs TLV is used in the TxInitRbfMessage to communicate the if confirmed inputs are required.
/// </remarks>
public class RequiredConfirmedInputsTlv() : Tlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS);