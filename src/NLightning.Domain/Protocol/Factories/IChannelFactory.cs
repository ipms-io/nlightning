namespace NLightning.Domain.Protocol.Factories;

using Channels;
using Node.Options;
using Messages;

public interface IChannelFactory
{
    Channel CreateChannelV1AsNonInitiator(OpenChannel1Message message, FeatureOptions negotiatedFeatures);
}