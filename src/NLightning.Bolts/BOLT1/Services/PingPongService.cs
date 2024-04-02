using NLightning.Bolts.BOLT1.Interfaces;
using NLightning.Bolts.BOLT1.Messages;
using NLightning.Bolts.Factories;

namespace NLightning.Bolts.BOLT1.Services;

public class PingPongService(TimeSpan networkTimeout) : IPingPongService
{
    private readonly TimeSpan _networkTimeout = networkTimeout;
    private readonly Random _random = new();

    private TaskCompletionSource<bool> _pongReceivedTaskSource = new();
    private PingMessage _pingMessage = (PingMessage)MessageFactory.CreatePingMessage();

    public event EventHandler<PingMessage>? PingMessageReadyEvent;
    public event EventHandler? DisconnectEvent;

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

            await Task.Delay(new Random().Next(30000, 300000), cancellationToken);

            _pongReceivedTaskSource = new();
            _pingMessage = (PingMessage)MessageFactory.CreatePingMessage();
        }
    }

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