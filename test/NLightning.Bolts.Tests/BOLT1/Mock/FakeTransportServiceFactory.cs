using System.Net.Sockets;

namespace NLightning.Bolts.Tests.BOLT1.Mock;

using Bolts.BOLT1.Interfaces;
using Common.Interfaces;

internal class FakeTransportServiceFactory : ITransportServiceFactory, ITestTransportServiceFactory
{
    public ITransportService CreateTransportService(ILogger logger, bool isInitiator, ReadOnlySpan<byte> s,
                                                    ReadOnlySpan<byte> rs, TcpClient tcpClient)
    {
        return CreateTransportService(logger, isInitiator, s.ToArray(), rs.ToArray(), tcpClient);
    }

    public virtual ITransportService CreateTransportService(ILogger logger, bool isInitiator, byte[] s, byte[] rs,
                                                            TcpClient tcpClient)
    {
#pragma warning disable CS8603 // Possible null reference return
        return default;
#pragma warning restore CS8603 // Possible null reference return
    }
}