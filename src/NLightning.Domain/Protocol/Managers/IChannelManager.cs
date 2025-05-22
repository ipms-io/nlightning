using NBitcoin;

namespace NLightning.Domain.Protocol.Managers;

using Messages.Interfaces;
using Node.Options;

public interface IChannelManager
{
    IChannelMessage HandleChannelMessage(IChannelMessage message, FeatureOptions negotiatedFeatures,
                                         PubKey peerPubKey);
}