using NLightning.Domain.Channels.Models;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Node.Options;
using NLightning.Domain.Protocol.Messages;

namespace NLightning.Domain.Channels.Interfaces;

public interface IChannelFactory
{
    Task<ChannelModel> CreateChannelV1AsNonInitiatorAsync(OpenChannel1Message message,
                                                          FeatureOptions negotiatedFeatures,
                                                          CompactPubKey remoteNodeId);
}