using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Domain.Channels.Interfaces;

using Crypto.ValueObjects;
using Node.Options;

public interface IChannelManager
{
    Task InitializeAsync();

    Task<IChannelMessage?> HandleChannelMessageAsync(IChannelMessage message, FeatureOptions negotiatedFeatures,
                                                     CompactPubKey peerPubKey);

    Task ForgetOldChannelByBlockHeightAsync(uint blockHeight);
}