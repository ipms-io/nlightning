using System.Net.Sockets;

namespace NLightning.Domain.Node.Interfaces;

using Crypto.ValueObjects;

/// <summary>
/// Interface for creating peer services.
/// </summary>
public interface IPeerServiceFactory
{
    /// <summary>
    /// Creates a peer we're connecting to.
    /// </summary>
    /// <param name="peerPubKey">Peer public key</param>
    /// <param name="tcpClient">TCP client</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created peer.</returns>
    Task<IPeerService> CreateConnectedPeerAsync(CompactPubKey peerPubKey, TcpClient tcpClient);

    /// <summary>
    /// Creates a peer connecting to us.
    /// </summary>
    /// <param name="tcpClient">TCP client</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created peer.</returns>
    Task<IPeerService> CreateConnectingPeerAsync(TcpClient tcpClient);
}