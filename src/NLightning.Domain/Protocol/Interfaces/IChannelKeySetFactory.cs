namespace NLightning.Domain.Protocol.Interfaces;

using Channels.Models;
using Payloads;

public interface IChannelKeySetFactory
{
    ChannelKeySetModel CreateNew();
    ChannelKeySetModel CreateFromIndex(uint index);
    ChannelKeySetModel CreateFromRemoteInfo(OpenChannel1Payload payload);
}