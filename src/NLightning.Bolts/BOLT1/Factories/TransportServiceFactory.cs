using System.Net.Sockets;

namespace NLightning.Bolts.BOLT1.Factories;

using BOLT8.Interfaces;
using BOLT8.Services;
using Interfaces;

public sealed class TransportServiceFactory : ITransportServiceFactory
{
    public ITransportService CreateTransportService(bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, TcpClient tcpClient)
    {
        return new TransportService(isInitiator, s, rs, tcpClient);
    }
}