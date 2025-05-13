using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NLightning.Infrastructure.Node;

using Application.Options;
using Common.Interfaces;
using Domain.Exceptions;
using Domain.Protocol.Interfaces;
using Domain.ValueObjects;
using Interfaces;

public class PeerFactory : IPeerFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMessageFactory _messageFactory;
    private readonly IMessageServiceFactory _messageServiceFactory;
    private readonly IPingPongServiceFactory _pingPongServiceFactory;
    private readonly ISecureKeyManager _secureKeyManager;
    private readonly ITransportServiceFactory _transportServiceFactory;
    private readonly NodeOptions _nodeOptions;

    public PeerFactory(ILoggerFactory loggerFactory, IMessageFactory messageFactory,
                       IMessageServiceFactory messageServiceFactory, IPingPongServiceFactory pingPongServiceFactory,
                       ISecureKeyManager secureKeyManager, ITransportServiceFactory transportServiceFactory,
                       IOptions<NodeOptions> nodeOptions)
    {
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
    /// <param name="peerAddress">Peer address</param>
    /// <param name="tcpClient">TCP client</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created peer.</returns>
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    public async Task<Peer> CreateConnectedPeerAsync(Protocol.Models.PeerAddress peerAddress, TcpClient tcpClient)
    {
        // Create a specific logger for the Peer class
        var logger = _loggerFactory.CreateLogger<Peer>();

        // Create and Initialize the transport service
        var keyBytes = _secureKeyManager.GetNodeKey().ToBytes();
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
        var key = _secureKeyManager.GetNodeKey();
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

        var peerAddress = new Protocol.Models.PeerAddress(transportService.RemoteStaticPublicKey, ipAddress, port);

        return new Peer(_nodeOptions.Features, logger, _messageFactory, messageService, _nodeOptions.NetworkTimeout,
                        peerAddress, pingPongService);
    }
}