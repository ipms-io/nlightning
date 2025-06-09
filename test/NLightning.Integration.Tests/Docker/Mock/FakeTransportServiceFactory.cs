using System.Net.Sockets;
using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Integration.Tests.Docker.Mock;

using Domain.Transport;

internal class FakeTransportServiceFactory : ITransportServiceFactory, ITestTransportServiceFactory
{
    public ITransportService CreateTransportService(bool isInitiator, ReadOnlySpan<byte> s,
                                                    ReadOnlySpan<byte> rs, TcpClient tcpClient)
    {
        return CreateTransportService(isInitiator, s.ToArray(), rs.ToArray(), tcpClient);
    }

    public virtual ITransportService CreateTransportService(bool isInitiator, byte[] s, byte[] rs,
                                                            TcpClient tcpClient)
    {
#pragma warning disable CS8603 // Possible null reference return
        return default;
#pragma warning restore CS8603 // Possible null reference return
    }
}