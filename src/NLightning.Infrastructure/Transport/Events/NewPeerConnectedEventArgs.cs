using System.Net.Sockets;

namespace NLightning.Infrastructure.Transport.Events;

public class NewPeerConnectedEventArgs : EventArgs
{
    public TcpClient TcpClient { get; set; }

    public NewPeerConnectedEventArgs(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
    }
}