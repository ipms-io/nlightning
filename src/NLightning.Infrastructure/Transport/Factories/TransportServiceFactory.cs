using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLightning.Application.Options;
using NLightning.Domain.Transport.Interfaces;
using NLightning.Infrastructure.Transport.Services;

namespace NLightning.Bolts.BOLT1.Factories;

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
    private readonly NodeOptions _nodeOptions;

    public TransportServiceFactory(ILoggerFactory loggerFactory, IOptions<NodeOptions> nodeOptions)
    {
        _loggerFactory = loggerFactory;
        _nodeOptions = nodeOptions.Value;
    }

    /// <inheritdoc />
    public ITransportService CreateTransportService(bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs, TcpClient tcpClient)
    {
        // Create a specific logger for the TransportService class
        var logger = _loggerFactory.CreateLogger<TransportService>();

        return new TransportService(logger, _nodeOptions.NetworkTimeout, isInitiator, s, rs, tcpClient);
    }
}