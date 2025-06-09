namespace NLightning.Domain.Protocol.Interfaces;

/// <summary>
/// Interface for a ping pong service.
/// </summary>
public interface IPingPongService
{
    /// <summary>
    /// Starts the ping service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task StartPingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Handles a pong message.
    /// </summary>
    /// <param name="message">The pong message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    void HandlePong(IMessage message);

    /// <summary>
    /// Event that is raised when a ping message is ready to be sent.
    /// </summary>
    event EventHandler<IMessage>? PingMessageReadyEvent;

    /// <summary>
    /// Event that is raised when the pong is not received in time or the pong message is invalid.
    /// </summary>
    event EventHandler<Exception>? DisconnectEvent;
}