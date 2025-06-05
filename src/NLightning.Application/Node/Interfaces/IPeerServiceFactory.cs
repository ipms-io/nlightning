using System.Net.Sockets;
using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Application.Node.Interfaces;

/// <summary>
/// Interface for creating peer services.
/// </summary>
public interface IPeerServiceFactory
{
    /// <summary>
    /// Creates a peer that we're connecting to.
    /// </summary>
    /// <param name="peerPubKey">Peer public key</param>
    /// <param name="tcpClient">TCP client</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created peer.</returns>
    Task<IPeerService> CreateConnectedPeerAsync(CompactPubKey peerPubKey, TcpClient tcpClient);

    /// <summary>
    /// Creates a peer that is connecting to us.
    /// </summary>
    /// <param name="tcpClient">TCP client</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created peer.</returns>
    Task<IPeerService> CreateConnectingPeerAsync(TcpClient tcpClient);
}