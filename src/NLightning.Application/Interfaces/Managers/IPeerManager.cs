using System.Net.Sockets;
using NBitcoin;

namespace NLightning.Application.Interfaces.Managers;

using Domain.ValueObjects;

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

    /// <summary>
    /// Disconnects a peer.
    /// </summary>
    /// <param name="pubKey">Pubkey of the peer</param>
    void DisconnectPeer(PubKey pubKey);
}