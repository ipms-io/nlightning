
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Node.Options;
using NLightning.Domain.Protocol.Messages.Interfaces;

namespace NLightning.Domain.Channels.Interfaces;

public interface IChannelManager
{
    Task InitializeAsync();
    
    IChannelMessage HandleChannelMessage(IChannelMessage message, FeatureOptions negotiatedFeatures,
                                         CompactPubKey peerPubKey);
}