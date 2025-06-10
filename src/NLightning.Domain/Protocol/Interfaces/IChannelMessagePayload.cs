namespace NLightning.Domain.Protocol.Interfaces;

using Channels.ValueObjects;

public interface IChannelMessagePayload : IMessagePayload
{
    ChannelId ChannelId { get; }
}