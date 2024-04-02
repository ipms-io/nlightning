using System.Net.Sockets;

namespace NLightning.Bolts.Tests.BOLT1.Mock;

using Bolts.BOLT8.Interfaces;

internal interface ITestTransportServiceFactory
{
    ITransportService CreateTransportService(bool isInitiator, byte[] s, byte[] rs, TcpClient tcpClient);
}