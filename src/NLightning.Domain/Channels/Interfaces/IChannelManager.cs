namespace NLightning.Domain.Channels.Interfaces;

using Crypto.ValueObjects;
using Node.Options;
using Protocol.Messages.Interfaces;

public interface IChannelManager
{
    Task InitializeAsync();

    Task<IChannelMessage?> HandleChannelMessageAsync(IChannelMessage message, FeatureOptions negotiatedFeatures,
                                                     CompactPubKey peerPubKey);

    Task ForgetOldChannelByBlockHeightAsync(uint blockHeight);
}