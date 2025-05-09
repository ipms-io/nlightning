using System.Net.Sockets;
using NLightning.Domain.ValueObjects;

namespace NLightning.Infrastructure.Node.Interfaces;

public interface IPeerFactory
{
    Task<Peer> CreateConnectedPeerAsync(PeerAddress peerAddress, TcpClient tcpClient);
    Task<Peer> CreateConnectingPeerAsync(TcpClient tcpClient);
}