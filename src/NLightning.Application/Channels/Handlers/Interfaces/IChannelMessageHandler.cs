namespace NLightning.Application.Channels.Handlers.Interfaces;

using Domain.Channels.Enums;
using Domain.Crypto.ValueObjects;
using Domain.Node.Options;
using Domain.Protocol.Messages.Interfaces;

/// <summary>
/// Base interface for all channel message handlers
/// </summary>
/// <typeparam name="TMessage">The type of message this handler can process</typeparam>
public interface IChannelMessageHandler<in TMessage> where TMessage : IChannelMessage
{
    /// <summary>
    /// Handles a channel message and returns a response message if needed
    /// </summary>
    /// <param name="message">The message to handle</param>
    /// <param name="currentState">The current state of the channel</param>
    /// <param name="negotiatedFeatures">Features negotiated with the peer</param>
    /// <param name="peerPubKey">The public key of the peer</param>
    /// <returns>A response message if needed, or null if no response is needed</returns>
    Task<IChannelMessage?> HandleAsync(TMessage message, ChannelState currentState, FeatureOptions negotiatedFeatures,
                                       CompactPubKey peerPubKey);
}