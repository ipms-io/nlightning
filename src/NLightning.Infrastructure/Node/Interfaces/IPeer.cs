namespace NLightning.Infrastructure.Node.Interfaces;

using Protocol.Models;

public interface IPeer
{
    event EventHandler? DisconnectEvent;
    PeerAddress PeerAddress { get; }
    void Disconnect();
}