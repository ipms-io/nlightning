using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Application.Node.Interfaces;

using Domain.Crypto.ValueObjects;

/// <summary>
/// Interface for communication with a single peer.
/// </summary>
public interface IPeerCommunicationService
{
    /// <summary>
    /// Gets a value indicating whether the connection is established.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the peer's public key.
    /// </summary>
    CompactPubKey PeerCompactPubKey { get; }

    /// <summary>
    /// Event raised when a message is received from the peer.
    /// </summary>
    event EventHandler<IMessage?> MessageReceived;

    /// <summary>
    /// Event raised when the peer is disconnected.
    /// </summary>
    event EventHandler? DisconnectEvent;

    /// <summary>
    /// Event raised when an exception occurs.
    /// </summary>
    event EventHandler<Exception>? ExceptionRaised;

    /// <summary>
    /// Sends a message to the peer.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the communication with the peer.
    /// </summary>
    /// <param name="networkTimeout">The network timeout.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InitializeAsync(TimeSpan networkTimeout);

    /// <summary>
    /// Disconnects from the peer.
    /// </summary>
    void Disconnect();
}