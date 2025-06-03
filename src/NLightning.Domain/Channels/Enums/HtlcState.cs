namespace NLightning.Domain.Channels.Enums;

public enum HtlcState : byte
{
    Offered = 0,
    Fulfilled = 1,
    Failed = 2,
    Expired = 3,
}