using System.Net.Sockets;

namespace NLightning.Bolts.BOLT1.Interfaces;

using BOLT1.Primitives;

/// <summary>
/// Interface for a peer service.
/// </summary>
public interface IPeerService

{
    /// <summary>
    /// Connects to a peer.
    /// </summary>
    /// <param name="peerAddress">The peer address to connect to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ConnectToPeerAsync(PeerAddress peerAddress);

    /// <summary>
    /// Accepts a peer.
    /// </summary>
    /// <param name="tcpClient">The TCP client.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AcceptPeerAsync(TcpClient tcpClient);
}