namespace NLightning.Bolts.BOLT1.Interfaces;

using Bolts.Interfaces;

/// <summary>
/// Interface for a message service.
/// </summary>
/// <remarks>
/// This interface is used to send and receive messages.
/// </remarks>
public interface IMessageService : IDisposable
{
    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Event that is raised when a message is received.
    /// </summary>
    event EventHandler<IMessage>? MessageReceived;

    /// <summary>
    /// Gets a value indicating whether the transport service is connected.
    /// </summary>
    bool IsConnected { get; }
}