using System.Net.Sockets;

namespace NLightning.Bolts.BOLT1.Interfaces;

using BOLT8.Interfaces;

public interface ITransportServiceFactory
{
    ITransportService CreateTransportService(bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, TcpClient tcpClient);
}