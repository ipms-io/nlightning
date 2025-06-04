using NLightning.Domain.Channels.Models;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Interfaces;

public interface IChannelKeySetFactory
{
    ChannelKeySetModel CreateNew();
    ChannelKeySetModel CreateFromIndex(uint index);
    ChannelKeySetModel CreateFromRemoteInfo(OpenChannel1Payload payload);
}