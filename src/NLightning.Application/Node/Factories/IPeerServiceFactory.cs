using System.Net.Sockets;
using NBitcoin;
using NLightning.Application.Node.Services.Interfaces;

namespace NLightning.Application.Node.Factories;
public interface IPeerServiceFactory
{
    Task<IPeerService> CreateConnectedPeerAsync(PubKey peerPubKey, TcpClient tcpClient);
    Task<IPeerService> CreateConnectingPeerAsync(TcpClient tcpClient);
}