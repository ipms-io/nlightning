using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace NLightning.Bolts.BOLT1.Factories;

using BOLT8.Services;
using Common.Interfaces;

/// <summary>
/// Factory for creating a transport service.
/// </summary>
/// <remarks>
/// This class is used to create a transport service in test environments.
/// </remarks>
public sealed class TransportServiceFactory : ITransportServiceFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public TransportServiceFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public ITransportService CreateTransportService(bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, TcpClient tcpClient)
    {
        // Create a specific logger for the TransportService class
        var logger = _loggerFactory.CreateLogger<TransportService>();

        return new TransportService(logger, isInitiator, s, rs, tcpClient);
    }
}