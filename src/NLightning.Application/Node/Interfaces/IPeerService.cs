using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Application.Node.Interfaces;

/// <summary>
/// Interface for the peer application service.
/// </summary>
public interface IPeerService
{
    /// <summary>
    /// Gets the peer's public key.
    /// </summary>
    CompactPubKey PeerPubKey { get; }

    /// <summary>
    /// Event raised when the peer is disconnected.
    /// </summary>
    event EventHandler? DisconnectEvent;

    /// <summary>
    /// Disconnects from the peer.
    /// </summary>
    void Disconnect();
}