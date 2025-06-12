using NLightning.Domain.Channels.Events;

namespace NLightning.Domain.Channels.Interfaces;

using Domain.Protocol.Interfaces;
using Crypto.ValueObjects;
using Node.Options;

public interface IChannelManager
{
    event EventHandler<ChannelResponseMessageEventArgs> OnResponseMessageReady;

    Task InitializeAsync();

    Task<IChannelMessage?> HandleChannelMessageAsync(IChannelMessage message, FeatureOptions negotiatedFeatures,
                                                     CompactPubKey peerPubKey);
}