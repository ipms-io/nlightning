namespace NLightning.Domain.Protocol.Enums;

public enum BasepointType : byte
{
    Funding = 0,
    Revocation = 1,
    Payment = 2,
    DelayedPayment = 3,
    Htlc = 4
}