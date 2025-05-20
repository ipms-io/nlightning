using System.Net.Sockets;

namespace NLightning.Infrastructure.Node.Interfaces;

using Models;
using Protocol.Models;

public interface IPeerFactory
{
    Task<Peer> CreateConnectedPeerAsync(PeerAddress peerAddress, TcpClient tcpClient);
    Task<Peer> CreateConnectingPeerAsync(TcpClient tcpClient);
}