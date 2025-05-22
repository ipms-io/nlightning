using NBitcoin;

namespace NLightning.Application.Node.Services.Interfaces;

public interface IPeerService
{
    void Disconnect();
    event EventHandler? DisconnectEvent;
    PubKey PeerPubKey { get; }
}