namespace NLightning.Domain.Protocol.Messages.Interfaces;

using Payloads.Interfaces;

public interface IChannelMessage : IMessage
{
    new IChannelMessagePayload Payload { get; }
}