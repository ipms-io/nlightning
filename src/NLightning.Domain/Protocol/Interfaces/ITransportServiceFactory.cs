using System.Net.Sockets;
using NLightning.Domain.Transport.Interfaces;

namespace NLightning.Common.Interfaces;

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