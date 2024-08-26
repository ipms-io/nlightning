using System.Net.Sockets;

namespace NLightning.Bolts.Tests.BOLT1.Mock;

using Bolts.BOLT1.Interfaces;
using Bolts.BOLT8.Interfaces;

internal class FakeTransportServiceFactory : ITransportServiceFactory, ITestTransportServiceFactory
{
    public ITransportService CreateTransportService(bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, TcpClient tcpClient)
    {
        return CreateTransportService(isInitiator, s.ToArray(), rs.ToArray(), tcpClient);
    }

    public virtual ITransportService CreateTransportService(bool isInitiator, byte[] s, byte[] rs, TcpClient tcpClient)
    {
#pragma warning disable CS8603 // Possible null reference return 
        return default;
#pragma warning restore CS8603 // Possible null reference return
    }
}