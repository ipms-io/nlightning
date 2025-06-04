using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLightning.Application.Node.Interfaces;
using NLightning.Application.Node.Services;
using NLightning.Domain.Channels.Interfaces;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Exceptions;
using NLightning.Domain.Node.Options;
using NLightning.Domain.Protocol.Factories;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Infrastructure.Node.Services;

namespace NLightning.Infrastructure.Node.Factories;

/// <summary>
/// Factory for creating peer services.
/// </summary>
public class PeerServiceFactory : IPeerServiceFactory
{
    private readonly IChannelManager _channelManager;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMessageFactory _messageFactory;
    private readonly IMessageServiceFactory _messageServiceFactory;
    private readonly IPingPongServiceFactory _pingPongServiceFactory;
    private readonly ISecureKeyManager _secureKeyManager;
    private readonly ITransportServiceFactory _transportServiceFactory;
    private readonly NodeOptions _nodeOptions;

    public PeerServiceFactory(IChannelManager channelManager, ILoggerFactory loggerFactory,
                              IMessageFactory messageFactory, IMessageServiceFactory messageServiceFactory,
                              IPingPongServiceFactory pingPongServiceFactory, ISecureKeyManager secureKeyManager,
                              ITransportServiceFactory transportServiceFactory, IOptions<NodeOptions> nodeOptions)
    {
        _channelManager = channelManager;
        _loggerFactory = loggerFactory;
        _messageFactory = messageFactory;
        _messageServiceFactory = messageServiceFactory;
        _pingPongServiceFactory = pingPongServiceFactory;
        _secureKeyManager = secureKeyManager;
        _transportServiceFactory = transportServiceFactory;
        _nodeOptions = nodeOptions.Value;
    }

    /// <summary>
    /// Creates a peer that we're connecting to.
    /// </summary>
    /// <param name="peerPubKey">Peer public key</param>
    /// <param name="tcpClient">TCP client</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created peer.</returns>
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    public async Task<IPeerService> CreateConnectedPeerAsync(CompactPubKey peerPubKey, TcpClient tcpClient)
    {
        // Create a specific logger for the communication service
        var commLogger = _loggerFactory.CreateLogger<PeerCommunicationService>();
        var appLogger = _loggerFactory.CreateLogger<PeerApplicationService>();

        // Create and Initialize the transport service
        var key = _secureKeyManager.GetNodeKeyPair();
        var transportService =
            _transportServiceFactory.CreateTransportService(true, key.PrivKey, peerPubKey, tcpClient);
        try
        {
            await transportService.InitializeAsync();
        }
        catch (Exception ex)
        {
            throw new ConnectionException($"Error connecting to peer {peerPubKey}", ex);
        }

        // Create the message service
        var messageService = _messageServiceFactory.CreateMessageService(transportService);

        // Create the ping pong service
        var pingPongService = _pingPongServiceFactory.CreatePingPongService();

        // Create the communication service (infrastructure layer)
        var communicationService =
            new PeerCommunicationService(commLogger, messageService, _messageFactory, peerPubKey, pingPongService);

        // Create the application service (application layer)
        return new PeerApplicationService(_channelManager, communicationService, _nodeOptions.Features, appLogger,
                                          _nodeOptions.NetworkTimeout);
    }

    /// <summary>
    /// Creates a peer that is connecting to us.
    /// </summary>
    /// <param name="tcpClient">TCP client</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created peer.</returns>
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    public async Task<IPeerService> CreateConnectingPeerAsync(TcpClient tcpClient)
    {
        // Create loggers
        var commLogger = _loggerFactory.CreateLogger<PeerCommunicationService>();
        var appLogger = _loggerFactory.CreateLogger<PeerApplicationService>();

        var remoteEndPoint =
            (IPEndPoint)(tcpClient.Client.RemoteEndPoint ?? throw new Exception("Failed to get remote endpoint"));
        var ipAddress = remoteEndPoint.Address.ToString();
        var port = remoteEndPoint.Port;

        // Create and Initialize the transport service
        var key = _secureKeyManager.GetNodeKeyPair();
        var transportService =
            _transportServiceFactory.CreateTransportService(false, key.PrivKey, key.CompactPubKey, tcpClient);
        try
        {
            await transportService.InitializeAsync();
        }
        catch (Exception ex)
        {
            throw new ConnectionException($"Error establishing connection to peer {ipAddress}:{port}", ex);
        }

        if (transportService.RemoteStaticPublicKey is null)
        {
            throw new ErrorException("Failed to get remote static public key");
        }

        // Create the message service
        var messageService = _messageServiceFactory.CreateMessageService(transportService);

        // Create the ping pong service
        var pingPongService = _pingPongServiceFactory.CreatePingPongService();

        // Create the communication service (infrastructure layer)
        var communicationService = new PeerCommunicationService(commLogger, messageService, _messageFactory,
                                                                transportService.RemoteStaticPublicKey.Value,
                                                                pingPongService);

        // Create the application service (application layer)
        return new PeerApplicationService(_channelManager, communicationService, _nodeOptions.Features, appLogger,
                                          _nodeOptions.NetworkTimeout);
    }
}