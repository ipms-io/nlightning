using System.Net.Sockets;
using NLightning.Bolts.BOLT1.Primitives;

namespace NLightning.Bolts.BOLT1.Interfaces;

public interface IPeerService

{
    Task ConnectToPeerAsync(PeerAddress peerAddress);
    Task AcceptPeerAsync(TcpClient tcpClient);
}