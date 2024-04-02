namespace NLightning.Bolts.BOLT1.Interfaces;

using Messages;

public interface IPingPongService
{
    Task StartPingAsync(CancellationToken cancellationToken);
    void HandlePong(PongMessage pongMessage);

    event EventHandler<PingMessage>? PingMessageReadyEvent;
    event EventHandler? DisconnectEvent;
}