namespace NLightning.Domain.Protocol.Factories;

using Channels;
using Messages;
using Node.Options;

public interface IChannelFactory
{
    Channel CreateChannelV1AsNonInitiator(OpenChannel1Message message, FeatureOptions negotiatedFeatures);
}