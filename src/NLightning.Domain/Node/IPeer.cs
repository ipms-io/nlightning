using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Node;

public interface IPeer
{
    event EventHandler? DisconnectEvent;
    PeerAddress PeerAddress { get; }
    void Disconnect();
}