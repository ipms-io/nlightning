using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NLightning.Infrastructure.Transport.Factories;

using Domain.Node.Options;
using Domain.Protocol.Interfaces;
using Domain.Serialization.Interfaces;
using Domain.Transport;
using Infrastructure.Crypto.Interfaces;
using Services;

/// <summary>
/// Factory for creating a transport service.
/// </summary>
/// <remarks>
/// This class is used to create a transport service in test environments.
/// </remarks>
public sealed class TransportServiceFactory : ITransportServiceFactory
{
    private readonly IEcdh _ecdh;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMessageSerializer _messageSerializer;
    private readonly NodeOptions _nodeOptions;

    public TransportServiceFactory(IEcdh ecdh, ILoggerFactory loggerFactory, IMessageSerializer messageSerializer,
                                   IOptions<NodeOptions> nodeOptions)
    {
        _ecdh = ecdh;
        _loggerFactory = loggerFactory;
        _messageSerializer = messageSerializer;
        _nodeOptions = nodeOptions.Value;
    }

    /// <inheritdoc />
    public ITransportService CreateTransportService(bool isInitiator, ReadOnlySpan<byte> s, ReadOnlySpan<byte> rs,
                                                    TcpClient tcpClient)
    {
        // Create a specific logger for the TransportService class
        var logger = _loggerFactory.CreateLogger<TransportService>();

        return new TransportService(_ecdh, logger, _messageSerializer, _nodeOptions.NetworkTimeout, isInitiator, s, rs,
                                    tcpClient);
    }
}