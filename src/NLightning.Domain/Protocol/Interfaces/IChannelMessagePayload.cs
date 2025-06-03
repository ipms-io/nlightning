using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads.Interfaces;

using Domain.ValueObjects;

public interface IChannelMessagePayload : IMessagePayload
{
    ChannelId ChannelId { get; }
}