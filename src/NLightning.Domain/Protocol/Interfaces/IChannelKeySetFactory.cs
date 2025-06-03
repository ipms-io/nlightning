using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Interfaces;

public interface IChannelKeySetFactory
{
    ChannelKeySet CreateNew();
    ChannelKeySet CreateFromIndex(uint index);
    ChannelKeySet CreateFromRemoteInfo(OpenChannel1Payload payload);
}