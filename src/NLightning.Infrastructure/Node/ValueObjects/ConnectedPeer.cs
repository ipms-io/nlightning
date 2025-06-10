using System.Net.Sockets;

namespace NLightning.Infrastructure.Node.ValueObjects;

using Domain.Crypto.ValueObjects;

public readonly record struct ConnectedPeer
{
    /// <summary>
    /// The public key of the peer we are connected to.
    /// </summary>
    public CompactPubKey CompactPubKey { get; }

    /// <summary>
    /// The TCP client representing the connection to the peer.
    /// </summary>
    public TcpClient TcpClient { get; }

    /// <summary>
    /// Represents a connected peer in the network, consisting of a compact public key and a TCP client.
    /// </summary>
    /// <param name="compactPubKey">The compact public key of the peer.</param>
    /// <param name="tcpClient">The TCP client representing the connection to the peer.</param>
    public ConnectedPeer(CompactPubKey compactPubKey, TcpClient tcpClient)
    {
        CompactPubKey = compactPubKey;
        TcpClient = tcpClient;
    }
}