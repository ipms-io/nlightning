using Microsoft.Extensions.Logging;

namespace NLightning.Application.Node.Managers;

using Domain.Channels.Interfaces;
using Domain.Crypto.ValueObjects;
using Domain.Exceptions;
using Domain.Node.Events;
using Domain.Node.Interfaces;
using Domain.Node.ValueObjects;
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
    private readonly Dictionary<CompactPubKey, IPeerService> _peers = [];

    private CancellationTokenSource? _cts;

    public PeerManager(IChannelManager channelManager, ILogger<PeerManager> logger,
                       IPeerServiceFactory peerServiceFactory, ITcpService tcpService)
    {
        _channelManager = channelManager;
        _logger = logger;
        _peerServiceFactory = peerServiceFactory;
        _tcpService = tcpService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _tcpService.OnNewPeerConnected += OnPeerConnected;

        await _tcpService.StartListeningAsync(_cts.Token);
    }

    public async Task StopAsync()
    {
        if (_cts is null)
            throw new InvalidOperationException($"{nameof(PeerManager)} is not running");

        foreach (var peerKey in _peers.Keys)
            DisconnectPeer(peerKey);

        while (_peers.Count > 0)
            await Task.Delay(TimeSpan.FromSeconds(1));

        await _cts.CancelAsync();
    }

    /// <inheritdoc />
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    public async Task ConnectToPeerAsync(PeerAddressInfo peerAddressInfo)
    {
        // Connect to the peer
        var connectedPeer = await _tcpService.ConnectToPeerAsync(peerAddressInfo);

        var peer = await _peerServiceFactory.CreateConnectedPeerAsync(connectedPeer.CompactPubKey,
                                                                      connectedPeer.TcpClient);
        peer.OnDisconnect += HandlePeerDisconnection;
        peer.OnChannelMessageReceived += HandlePeerChannelMessage;

        _peers.Add(connectedPeer.CompactPubKey, peer);
    }

    /// <inheritdoc />
    public void DisconnectPeer(CompactPubKey pubKey)
    {
        if (_peers.TryGetValue(pubKey, out var peer))
        {
            peer.Disconnect();
        }
        else
        {
            _logger.LogWarning("Peer {Peer} not found", pubKey);
        }
    }

    private void OnPeerConnected(object? _, NewPeerConnectedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args.TcpClient);

        // Create the peer
        _peerServiceFactory.CreateConnectingPeerAsync(args.TcpClient).ContinueWith(task =>
        {
            if (task.IsFaulted)
                _logger.LogError(task.Exception, "Error creating a connecting peer");

            var peer = task.Result;

            peer.OnDisconnect += HandlePeerDisconnection;
            peer.OnChannelMessageReceived += HandlePeerChannelMessage;

            _peers.Add(peer.PeerPubKey, peer);
        });
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

        if (!_peers.TryGetValue(args.PeerPubKey, out var peerService))
            throw new ConnectionException("Peer not found");

        _channelManager.HandleChannelMessageAsync(args.Message, peerService.Features, peerService.PeerPubKey)
                       .ContinueWith(task => HandleChannelMessageResponseAsync(task, peerService.PeerPubKey,
                                                                               args.Message.Type));
    }

    private async Task HandleChannelMessageResponseAsync(Task<IChannelMessage?> task, CompactPubKey peerPubKey,
                                                         MessageTypes messageType)
    {
        if (!_peers.TryGetValue(peerPubKey, out var peerService))
            throw new ConnectionException("Peer not found");

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
}