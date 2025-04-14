using System.Net.Sockets;

namespace NLightning.Bolts.BOLT1.Factories;

using BOLT8.Services;
using Common.Interfaces;
using Interfaces;

/// <summary>
/// Factory for creating a transport service.
/// </summary>
/// <remarks>
/// This class is used to create a transport service in test environments.
/// </remarks>
public sealed class TransportServiceFactory : ITransportServiceFactory
{
    /// <inheritdoc />
    public ITransportService CreateTransportService(ILogger logger, bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, TcpClient tcpClient)
    {
        return new TransportService(logger, isInitiator, s, rs, tcpClient);
    }
}