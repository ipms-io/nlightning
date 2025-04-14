using System.Net.Sockets;

namespace NLightning.Bolts.Tests.BOLT1.Mock;

using Common.Interfaces;

internal interface ITestTransportServiceFactory
{
    ITransportService CreateTransportService(ILogger logger, bool isInitiator, byte[] s, byte[] rs, TcpClient tcpClient);
}