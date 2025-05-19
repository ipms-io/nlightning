using System.Net.Sockets;

namespace NLightning.Integration.Tests.Docker.Mock;

using Domain.Transport;

internal interface ITestTransportServiceFactory
{
    ITransportService CreateTransportService(bool isInitiator, byte[] s, byte[] rs, TcpClient tcpClient);
}