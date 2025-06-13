using System.Net.Sockets;

namespace NLightning.Infrastructure.Transport.Events;

public class NewPeerConnectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the host address of the connected peer.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the port number of the connected peer.
    /// </summary>
    public uint Port { get; }

    /// <summary>
    /// Gets the TCP client associated with the newly connected peer.
    /// </summary>
    public TcpClient TcpClient { get; }

    public NewPeerConnectedEventArgs(string host, uint port, TcpClient tcpClient)
    {
        Host = host;
        Port = port;
        TcpClient = tcpClient;
    }
}