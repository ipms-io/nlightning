using Microsoft.Extensions.Options;

namespace NLightning.Bolts.BOLT1.Services;

using Common.Interfaces;
using Common.Messages;
using Common.Options;

/// <summary>
/// Service for managing the ping pong protocol.
/// </summary>
/// <remarks>
/// This class is used to manage the ping pong protocol.
/// </remarks>
internal class PingPongService : IPingPongService
{
    private readonly IMessageFactory _messageFactory;
    private readonly NodeOptions _nodeOptions;
    private readonly Random _random = new();

    private TaskCompletionSource<bool> _pongReceivedTaskSource = new();
    private PingMessage _pingMessage;

    /// <inheritdoc />
    public event EventHandler<IMessage>? PingMessageReadyEvent;

    /// <inheritdoc />
    public event EventHandler<Exception>? DisconnectEvent;

    public PingPongService(IMessageFactory messageFactory, IOptions<NodeOptions> nodeOptions)
    {
        _messageFactory = messageFactory;
        _nodeOptions = nodeOptions.Value;
        _pingMessage = (PingMessage)messageFactory.CreatePingMessage();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Ping messages are sent to the peer at random intervals ranging from 30 seconds to 5 minutes.
    /// If a pong message is not received within the network timeout, DisconnectEvent is raised.
    /// </remarks>
    public async Task StartPingAsync(CancellationToken cancellationToken)
    {
        // Send the first ping message
        while (!cancellationToken.IsCancellationRequested)
        {
            PingMessageReadyEvent?.Invoke(this, _pingMessage);

            using var pongTimeoutTokenSource = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken,
                                         new CancellationTokenSource(_nodeOptions.NetworkTimeout).Token);

            var task = await Task.WhenAny(_pongReceivedTaskSource.Task, Task.Delay(-1, pongTimeoutTokenSource.Token));
            if (task.IsFaulted)
            {
                DisconnectEvent?
                    .Invoke(this, new ConnectionException("Pong message not received within network timeout."));
                return;
            }

            if (task.IsCanceled)
            {
                continue;
            }

            await Task.Delay(_random.Next(30_000, 300_000), cancellationToken);

            _pongReceivedTaskSource = new TaskCompletionSource<bool>();
            _pingMessage = (PingMessage)_messageFactory.CreatePingMessage();
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Handles a pong message.
    /// If the pong message has a different length than the ping message, DisconnectEvent is raised.
    /// </remarks>
    public void HandlePong(IMessage message)
    {
        // if the pong message has a different length than the ping message, disconnect
        if (message is not PongMessage pongMessage ||
            pongMessage.Payload.BytesLength != _pingMessage.Payload.NumPongBytes)
        {
            DisconnectEvent?.Invoke(this, new Exception("Pong message has different length than ping message."));
            return;
        }

        _pongReceivedTaskSource.TrySetResult(true);
    }
}