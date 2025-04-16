using System.Net.Sockets;

namespace NLightning.Bolts.Tests.Mock;

using Common.Interfaces;

internal interface ITestTransportServiceFactory
{
    ITransportService CreateTransportService(bool isInitiator, byte[] s, byte[] rs, TcpClient tcpClient);
}