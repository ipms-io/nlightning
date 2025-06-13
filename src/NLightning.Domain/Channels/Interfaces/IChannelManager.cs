namespace NLightning.Domain.Channels.Interfaces;

using Crypto.ValueObjects;
using Domain.Protocol.Interfaces;
using Events;
using Models;
using Node.Options;

public interface IChannelManager
{
    event EventHandler<ChannelResponseMessageEventArgs> OnResponseMessageReady;

    Task<IChannelMessage?> HandleChannelMessageAsync(IChannelMessage message, FeatureOptions negotiatedFeatures,
                                                     CompactPubKey peerPubKey);

    Task RegisterExistingChannelAsync(ChannelModel channel);
}