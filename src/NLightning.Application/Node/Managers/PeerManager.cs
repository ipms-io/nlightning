using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NLightning.Application.Node.Managers;

using Domain.Channels.Enums;
using Domain.Channels.Events;
using Domain.Channels.Interfaces;
using Domain.Crypto.ValueObjects;
using Domain.Exceptions;
using Domain.Node.Events;
using Domain.Node.Interfaces;
using Domain.Node.Models;
using Domain.Node.ValueObjects;
using Domain.Persistence.Interfaces;
using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Infrastructure.Transport.Events;
using Infrastructure.Transport.Interfaces;

/// <summary>
/// Service for managing peers.
/// </summary>
/// <remarks>
/// This class is used to manage peers in the network.
/// </remarks>
/// <seealso cref="IPeerManager" />
public sealed class PeerManager : IPeerManager
{
    private readonly IChannelManager _channelManager;
    private readonly ILogger<PeerManager> _logger;
    private readonly IPeerServiceFactory _peerServiceFactory;
    private readonly ITcpService _tcpService;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<CompactPubKey, PeerModel> _peers = [];

    private CancellationTokenSource? _cts;

    public PeerManager(IChannelManager channelManager, ILogger<PeerManager> logger,
                       IPeerServiceFactory peerServiceFactory, ITcpService tcpService, IServiceProvider serviceProvider)
    {
        _channelManager = channelManager;
        _logger = logger;
        _peerServiceFactory = peerServiceFactory;
        _tcpService = tcpService;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _tcpService.OnNewPeerConnected += HandleNewPeerConnected;

        _channelManager.OnResponseMessageReady += HandleResponseMessageReady;

        // Load peers and initialize the channel manager
        using var scope = _serviceProvider.CreateScope();
        using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var peers = await uow.GetPeersForStartupAsync();
        foreach (var peer in peers)
        {
            await ConnectToPeerAsync(peer.PeerAddressInfo, uow);
            if (!_peers.TryGetValue(peer.NodeId, out _))
            {
                _logger.LogWarning("Unable to connect to peer {PeerId} on startup", peer.NodeId);
                // TODO: Handle this case, maybe retry or log more details
                continue;
            }

            // Register channels with peer
            if (peer.Channels is not { Count: > 0 })
                continue;

            // Only register channels that are not closed or stale
            foreach (var channel in peer.Channels.Where(c => c.State != ChannelState.Closed))
                // We don't care about the result here, as we just want to register the existing channels
                _ = _channelManager.RegisterExistingChannelAsync(channel);
        }

        await uow.SaveChangesAsync();

        await _tcpService.StartListeningAsync(_cts.Token);
    }

    public async Task StopAsync()
    {
        if (_cts is null)
            throw new InvalidOperationException($"{nameof(PeerManager)} is not running");

        foreach (var peerKey in _peers.Keys)
            DisconnectPeer(peerKey);

        try
        {
            // Give it a 5-second timeout to disconnect all peers
            var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (_peers.Count > 0 && !_cts.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromSeconds(1), timeoutTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Timeout while waiting for peers to disconnect");
        }

        await _cts.CancelAsync();
    }

    /// <inheritdoc />
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    public async Task ConnectToPeerAsync(PeerAddressInfo peerAddressInfo)
    {
        using var scope = _serviceProvider.CreateScope();
        using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await ConnectToPeerAsync(peerAddressInfo, uow);

        await uow.SaveChangesAsync();
    }

    /// <inheritdoc />
    public void DisconnectPeer(CompactPubKey pubKey)
    {
        if (_peers.TryGetValue(pubKey, out var peer))
        {
            if (peer.TryGetPeerService(out var peerService))
            {
                peerService.Disconnect();
            }
            else
            {
                _logger.LogWarning("PeerService not found for {Peer}", pubKey);
            }
        }
        else
        {
            _logger.LogWarning("Peer {Peer} not found", pubKey);
        }
    }

    private async Task ConnectToPeerAsync(PeerAddressInfo peerAddressInfo, IUnitOfWork uow)
    {
        // Connect to the peer
        var connectedPeer = await _tcpService.ConnectToPeerAsync(peerAddressInfo);

        var peerService = await _peerServiceFactory.CreateConnectedPeerAsync(connectedPeer.CompactPubKey,
                                                                             connectedPeer.TcpClient);
        peerService.OnDisconnect += HandlePeerDisconnection;
        peerService.OnChannelMessageReceived += HandlePeerChannelMessage;

        // Check if the peer wants us to use a different host and port
        var host = connectedPeer.Host;
        var port = connectedPeer.Port; // Default port for Lightning Network
        if (peerService.PreferredHost is not null && peerService.PreferredPort.HasValue)
        {
            host = peerService.PreferredHost;
            port = peerService.PreferredPort.Value;
        }

        var peer = new PeerModel(connectedPeer.CompactPubKey, host, port)
        {
            LastSeenAt = DateTime.UtcNow
        };
        peer.SetPeerService(peerService);

        _peers.Add(connectedPeer.CompactPubKey, peer);

        uow.PeerDbRepository.AddOrUpdateAsync(peer).GetAwaiter().GetResult();
    }

    private void HandleNewPeerConnected(object? _, NewPeerConnectedEventArgs args)
    {
        try
        {
            // Create the peer
            var peerService = _peerServiceFactory.CreateConnectingPeerAsync(args.TcpClient).GetAwaiter().GetResult();

            peerService.OnDisconnect += HandlePeerDisconnection;
            peerService.OnChannelMessageReceived += HandlePeerChannelMessage;

            var peer = new PeerModel(peerService.PeerPubKey, args.Host, args.Port)
            {
                LastSeenAt = DateTime.UtcNow
            };
            peer.SetPeerService(peerService);

            _peers.Add(peerService.PeerPubKey, peer);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error handling new peer connection from {Host}:{Port}", args.Host, args.Port);
        }
    }

    private void HandlePeerDisconnection(object? _, PeerDisconnectedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        _peers.Remove(args.PeerPubKey);
        _logger.LogInformation("Peer {Peer} disconnected", args.PeerPubKey);
    }

    private void HandlePeerChannelMessage(object? _, ChannelMessageEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!_peers.TryGetValue(args.PeerPubKey, out var peer))
            throw new ConnectionException($"Peer {args.PeerPubKey} not found while handling channel message");

        if (!peer.TryGetPeerService(out var peerService))
            throw new ConnectionException(
                $"PeerService not found for peer {args.PeerPubKey} while handling channel message");

        _channelManager.HandleChannelMessageAsync(args.Message, peerService.Features, peerService.PeerPubKey)
                       .ContinueWith(task => HandleChannelMessageResponseAsync(task, peerService.PeerPubKey,
                                                                               args.Message.Type));
    }

    private async Task HandleChannelMessageResponseAsync(Task<IChannelMessage?> task, CompactPubKey peerPubKey,
                                                         MessageTypes messageType)
    {
        if (!_peers.TryGetValue(peerPubKey, out var peer))
            throw new ConnectionException($"Peer {peerPubKey} not found while handling channel response message");

        if (!peer.TryGetPeerService(out var peerService))
            throw new ConnectionException(
                $"PeerService not found for peer {peerPubKey} while handling channel response message");

        if (task.IsFaulted)
        {
            if (task.Exception is { InnerException: ChannelErrorException cee })
            {
                _logger.LogError(
                    "Error handling channel message ({messageType}) from peer {peer}: {message}",
                    Enum.GetName(messageType), peerService.PeerPubKey,
                    !string.IsNullOrEmpty(cee.PeerMessage)
                        ? cee.PeerMessage
                        : cee.Message);

                DisconnectPeer(peerService.PeerPubKey);
                return;
            }

            if (task.Exception is { InnerException: ChannelWarningException cwe })
            {
                _logger.LogWarning(
                    "Error handling channel message ({messageType}) from peer {peer}: {message}",
                    Enum.GetName(messageType), peerService.PeerPubKey,
                    !string.IsNullOrEmpty(cwe.PeerMessage)
                        ? cwe.PeerMessage
                        : cwe.Message);

                return;
            }

            _logger.LogError(
                task.Exception, "Error handling channel message ({messageType}) from peer {peer}",
                Enum.GetName(messageType), peerService.PeerPubKey);

            DisconnectPeer(peerService.PeerPubKey);
            return;
        }

        var replyMessage = task.Result;
        if (replyMessage is not null)
        {
            await peerService.SendMessageAsync(replyMessage);
        }
    }

    private void HandleResponseMessageReady(object? sender, ChannelResponseMessageEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        // Find PeerService by CompactPubKey
        if (!_peers.TryGetValue(args.PeerPubKey, out var peer))
            throw new ConnectionException($"Peer {args.PeerPubKey} not found while handling response message");

        if (!peer.TryGetPeerService(out var peerService))
            throw new ConnectionException(
                $"PeerService not found for peer {args.PeerPubKey} while handling response message");

        // Send the response message to the peer
        peerService.SendMessageAsync(args.ResponseMessage)
                   .ContinueWith(task =>
                    {
                        _logger.LogError(task.Exception, "Failed to send response message to peer {Peer}",
                                         args.PeerPubKey);
                    }, TaskContinuationOptions.OnlyOnFaulted);
    }
}