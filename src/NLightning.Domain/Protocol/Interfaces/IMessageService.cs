namespace NLightning.Domain.Protocol.Interfaces;

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
    /// <param name="throwOnException">If true, exceptions will be thrown instead of being raised via the OnExceptionRaised event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendMessageAsync(IMessage message, bool throwOnException = false,
                          CancellationToken cancellationToken = default);

    /// <summary>
    /// Event that is raised when a message is received.
    /// </summary>
    event EventHandler<IMessage?>? OnMessageReceived;

    /// <summary>
    /// Event that is raised when an exception is raised.
    /// </summary>
    event EventHandler<Exception>? OnExceptionRaised;

    /// <summary>
    /// Gets a value indicating whether the transport service is connected.
    /// </summary>
    bool IsConnected { get; }
}