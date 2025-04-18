using System.Net.Sockets;

namespace NLightning.Common.Interfaces;

using Node;
using Types;

public interface IPeerFactory
{
    Task<Peer> CreateConnectedPeerAsync(PeerAddress peerAddress, TcpClient tcpClient);
    Task<Peer> CreateConnectingPeerAsync(TcpClient tcpClient);
}