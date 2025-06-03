namespace NLightning.Domain.Channels.Enums;

public enum ChannelState : byte
{
    None = 0,
    V1Opening = 1,
    V1FundingCreated = 2,
    V1FundingSigned = 3,
    V2Opening = 10,
    Open = 20,
    Closing = 30,
    Closed = 40
}