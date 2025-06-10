namespace NLightning.Domain.Node.Interfaces;

using Crypto.ValueObjects;
using Domain.Protocol.Interfaces;
using Events;
using Options;

/// <summary>
/// Interface for the peer application service.
/// </summary>
public interface IPeerService
{
    /// <summary>
    /// Gets the peer's public key.
    /// </summary>
    CompactPubKey PeerPubKey { get; }

    /// <summary>
    /// Gets the feature options for the peer.
    /// </summary>
    FeatureOptions Features { get; }

    /// <summary>
    /// Event raised when the peer is disconnected.
    /// </summary>
    event EventHandler<PeerDisconnectedEventArgs> OnDisconnect;

    /// <summary>
    /// Occurs when a channel message is received from the connected peer.
    /// </summary>
    event EventHandler<ChannelMessageEventArgs> OnChannelMessageReceived;

    /// <summary>
    /// Disconnects from the peer.
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Sends an asynchronous message to the peer.
    /// </summary>
    /// <param name="replyMessage">The message to be sent to the peer.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendMessageAsync(IChannelMessage replyMessage);
}