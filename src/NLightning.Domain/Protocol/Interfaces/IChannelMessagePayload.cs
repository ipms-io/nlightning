using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads.Interfaces;
public interface IChannelMessagePayload : IMessagePayload
{
    ChannelId ChannelId { get; }
}