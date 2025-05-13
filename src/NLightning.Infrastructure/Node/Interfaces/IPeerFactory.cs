using System.Net.Sockets;
using NLightning.Domain.ValueObjects;
using PeerAddress = NLightning.Infrastructure.Protocol.Models.PeerAddress;

namespace NLightning.Infrastructure.Node.Interfaces;

public interface IPeerFactory
{
    Task<Peer> CreateConnectedPeerAsync(PeerAddress peerAddress, TcpClient tcpClient);
    Task<Peer> CreateConnectingPeerAsync(TcpClient tcpClient);
}