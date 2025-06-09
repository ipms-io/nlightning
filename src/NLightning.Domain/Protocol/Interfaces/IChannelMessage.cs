namespace NLightning.Domain.Protocol.Interfaces;

public interface IChannelMessage : IMessage
{
    new IChannelMessagePayload Payload { get; }
}