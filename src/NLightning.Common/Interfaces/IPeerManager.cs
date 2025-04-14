using System.Net.Sockets;

namespace NLightning.Common.Interfaces;

using Types;

/// <summary>
/// Interface for the peer manager.
/// </summary>
public interface IPeerManager

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