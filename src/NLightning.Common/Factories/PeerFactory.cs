using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NLightning.Common.Factories;

using Exceptions;
using Interfaces;
using Managers;
using Node;
using Options;
using Types;

public class PeerFactory : IPeerFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMessageFactory _messageFactory;
    private readonly IMessageServiceFactory _messageServiceFactory;
    private readonly IPingPongServiceFactory _pingPongServiceFactory;
    private readonly ITransportServiceFactory _transportServiceFactory;
    private readonly NodeOptions _nodeOptions;

    public PeerFactory(ILoggerFactory loggerFactory, IMessageFactory messageFactory,
                       IMessageServiceFactory messageServiceFactory, IPingPongServiceFactory pingPongServiceFactory,
                       ITransportServiceFactory transportServiceFactory, IOptions<NodeOptions> nodeOptions)
    {
        _loggerFactory = loggerFactory;
        _messageFactory = messageFactory;
        _messageServiceFactory = messageServiceFactory;
        _pingPongServiceFactory = pingPongServiceFactory;
        _transportServiceFactory = transportServiceFactory;
        _nodeOptions = nodeOptions.Value;
    }

    /// <summary>
    /// Creates a peer that we're connecting to.
    /// </summary>
    /// <param name="peerAddress">Peer address</param>
    /// <param name="tcpClient">TCP client</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created peer.</returns>
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    public async Task<Peer> CreateConnectedPeerAsync(PeerAddress peerAddress, TcpClient tcpClient)
    {
        // Create a specific logger for the Peer class
        var logger = _loggerFactory.CreateLogger<Peer>();

        // Create and Initialize the transport service
        var keyBytes = SecureKeyManager.GetPrivateKeyBytes();
        var transportService = _transportServiceFactory
            .CreateTransportService(true, keyBytes, peerAddress.PubKey.ToBytes(), tcpClient);
        try
        {
            await transportService.InitializeAsync();
        }
        catch (Exception ex)
        {
            throw new ConnectionException($"Error connecting to peer {peerAddress.Host}:{peerAddress.Port}", ex);
        }

        // Create the message service
        var messageService = _messageServiceFactory.CreateMessageService(transportService);

        // Create the ping pong service
        var pingPongService = _pingPongServiceFactory.CreatePingPongService();

        return new Peer(_nodeOptions.Features, logger, _messageFactory, messageService, _nodeOptions.NetworkTimeout,
                        peerAddress, pingPongService);
    }

    public async Task<Peer> CreateConnectingPeerAsync(TcpClient tcpClient)
    {
        // Create a specific logger for the Peer class
        var logger = _loggerFactory.CreateLogger<Peer>();

        var remoteEndPoint = (IPEndPoint)(tcpClient.Client.RemoteEndPoint
                                          ?? throw new Exception("Failed to get remote endpoint"));
        var ipAddress = remoteEndPoint.Address.ToString();
        var port = remoteEndPoint.Port;

        // Create and Initialize the transport service
        var key = SecureKeyManager.GetPrivateKey();
        var transportService = _transportServiceFactory
            .CreateTransportService(false, key.ToBytes(), key.PubKey.ToBytes(), tcpClient);
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

        var peerAddress = new PeerAddress(transportService.RemoteStaticPublicKey, ipAddress, port);

        return new Peer(_nodeOptions.Features, logger, _messageFactory, messageService, _nodeOptions.NetworkTimeout,
                        peerAddress, pingPongService);
    }
}