namespace NLightning.Domain.Channels.Interfaces;

using Crypto.ValueObjects;
using Models;
using Node.Options;
using Protocol.Messages;

public interface IChannelFactory
{
    Task<ChannelModel> CreateChannelV1AsNonInitiatorAsync(OpenChannel1Message message,
                                                          FeatureOptions negotiatedFeatures,
                                                          CompactPubKey remoteNodeId);
}