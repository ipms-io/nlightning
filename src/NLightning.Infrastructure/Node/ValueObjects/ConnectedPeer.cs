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
    /// The host address of the connected peer.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// The port used by the peer to establish the connection.
    /// </summary>
    public uint Port { get; }

    /// <summary>
    /// The TCP client representing the connection to the peer.
    /// </summary>
    public TcpClient TcpClient { get; }

    /// <summary>
    /// Represents a connected peer in the network, consisting of a compact public key and a TCP client.
    /// </summary>
    /// <param name="compactPubKey">The compact public key of the peer.</param>
    /// <param name="tcpClient">The TCP client representing the connection to the peer.</param>
    public ConnectedPeer(CompactPubKey compactPubKey, string host, uint port, TcpClient tcpClient)
    {
        CompactPubKey = compactPubKey;
        Host = host;
        Port = port;
        TcpClient = tcpClient;
    }
}