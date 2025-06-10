using System.Net.Sockets;

namespace NLightning.Domain.Protocol.Interfaces;

using Transport;

/// <summary>
/// Interface for a transport service factory.
/// </summary>
/// <remarks>
/// This interface is used to create a transport service in test environments.
/// </remarks>
public interface ITransportServiceFactory
{
    /// <summary>
    /// Creates a transport service.
    /// </summary>
    ITransportService CreateTransportService(bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs,
                                             TcpClient tcpClient);
}