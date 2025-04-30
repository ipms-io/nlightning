using NLightning.Domain.Protocol.Messages.Interfaces;

namespace NLightning.Domain.Protocol.Managers;

using Common.Interfaces;
using Node.Options;
public interface IChannelManager
{
    IChannelMessage? HandleChannelMessage(IChannelMessage message, FeatureOptions negotiatedFeatures);
}