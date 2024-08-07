namespace NLightning.Bolts.BOLT1.Services;

using Bolts.Factories;
using Interfaces;
using Messages;

/// <summary>
/// Service for managing the ping pong protocol.
/// </summary>
/// <remarks>
/// This class is used to manage the ping pong protocol.
/// </remarks>
/// <param name="networkTimeout">The network timeout.</param>
public class PingPongService(TimeSpan networkTimeout) : IPingPongService
{
    private readonly TimeSpan _networkTimeout = networkTimeout;
    private readonly Random _random = new();

    private TaskCompletionSource<bool> _pongReceivedTaskSource = new();
    private PingMessage _pingMessage = (PingMessage)MessageFactory.CreatePingMessage();

    /// <inheritdoc />
    public event EventHandler<PingMessage>? PingMessageReadyEvent;

    /// <inheritdoc />
    public event EventHandler? DisconnectEvent;

    /// <inheritdoc />
    /// <remarks>
    /// Ping messages are sent to the peer at random intervals ranging from 30 seconds to 5 minutes.
    /// If a pong message is not received within the network timeout, DisconnectEvent is raised.
    /// </remarks>
    public async Task StartPingAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            PingMessageReadyEvent?.Invoke(this, _pingMessage);

            using var pongTimeoutTokenSource = new CancellationTokenSource(_networkTimeout);

            if (await Task.WhenAny(_pongReceivedTaskSource.Task, Task.Delay(-1, pongTimeoutTokenSource.Token)) != _pongReceivedTaskSource.Task)
            {
                DisconnectEvent?.Invoke(this, EventArgs.Empty);
                return;
            }

            await Task.Delay(_random.Next(30000, 300000), cancellationToken);

            _pongReceivedTaskSource = new();
            _pingMessage = (PingMessage)MessageFactory.CreatePingMessage();
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Handles a pong message.
    /// If the pong message has a different length than the ping message, DisconnectEvent is raised.
    /// </remarks>
    public void HandlePong(PongMessage pongMessage)
    {
        // if the pong message has a different length than the ping message, disconnect
        if (pongMessage.Payload.BytesLength != _pingMessage.Payload.NumPongBytes)
        {
            DisconnectEvent?.Invoke(this, EventArgs.Empty);
            return;
        }

        _pongReceivedTaskSource.TrySetResult(true);
    }
}