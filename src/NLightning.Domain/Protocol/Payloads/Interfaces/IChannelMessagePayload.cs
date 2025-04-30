namespace NLightning.Domain.Protocol.Payloads.Interfaces;

using ValueObjects;

public interface IChannelMessagePayload : IMessagePayload
{
    ChannelId ChannelId { get; }
}