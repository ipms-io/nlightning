namespace NLightning.Domain.Protocol.Tlv;

using Constants;

/// <summary>
/// Required Confirmed Inputs TLV.
/// </summary>
/// <remarks>
/// The required confirmed inputs TLV is used in the TxInitRbfMessage to communicate if confirmed inputs are required.
/// </remarks>
public class RequireConfirmedInputsTlv() : BaseTlv(TlvConstants.RequireConfirmedInputs);