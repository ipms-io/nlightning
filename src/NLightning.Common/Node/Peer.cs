using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NLightning.Common.Node;

using Constants;
using Exceptions;
using Interfaces;
using Managers;
using Messages;
using Options;
using TLVs;
using Types;

/// <summary>
/// Represents a peer in the network.
/// </summary>
/// <remarks>
/// This class is used to communicate with a peer in the network.
/// </remarks>
public sealed class Peer
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IMessageService _messageService;
    private readonly IPingPongService _pingPongService;
    private readonly IMessageFactory _messageFactory;
    private readonly ILogger<Peer> _logger;
    private readonly NodeOptions _nodeOptions;
    private readonly bool _isInbound;

    private bool _isInitialized;

    /// <summary>
    /// Event raised when the peer is disconnected.
    /// </summary>
    public event EventHandler? DisconnectEvent;

    public PeerAddress PeerAddress { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Peer"/> class.
    /// </summary>
    /// <param name="messageService">The message service.</param>
    /// <param name="pingPongService">The ping pong service.</param>
    /// <param name="logger">A logger</param>
    /// <param name="messageFactory">The message factory</param>
    /// <param name="nodeOptions">The node options</param>
    /// <param name="peerAddress">Peer address</param>
    /// <param name="isInbound">A value indicating whether the peer is inbound.</param>
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    internal Peer(IMessageService messageService, IPingPongService pingPongService, IMessageFactory messageFactory,
                  ILogger<Peer> logger, IOptions<NodeOptions> nodeOptions, PeerAddress peerAddress, bool isInbound)
    {
        _messageService = messageService;
        _pingPongService = pingPongService;
        _messageFactory = messageFactory;
        _logger = logger;
        _nodeOptions = nodeOptions.Value;
        _isInbound = isInbound;

        PeerAddress = peerAddress;

        _messageService.MessageReceived += HandleMessage;
        _messageService.ExceptionRaised += HandleException;
        _pingPongService.DisconnectEvent += HandleException;

        // Always send an init message upon connection
        logger.LogTrace("[Peer] Sending init message to peer {peer}", PeerAddress.PubKey);
        var initMessage = _messageFactory.CreateInitMessage(_nodeOptions);
        _messageService.SendMessageAsync(initMessage, _cancellationTokenSource.Token).Wait();

        // Wait for an init message
        logger.LogTrace("[Peer] Waiting for init message from peer {peer}", PeerAddress.PubKey);
        // Set timeout to close connection if the other peer doesn't send an init message
        Task.Delay(ConfigManager.Instance.NetworkTimeout, _cancellationTokenSource.Token).ContinueWith(task =>
        {
            if (!task.IsCanceled && !_isInitialized)
            {
                DisconnectWithException(new ConnectionException("Peer did not send init message after timeout"));
            }
        });

        if (!_messageService.IsConnected)
        {
            throw new ConnectionException("Failed to connect to peer");
        }
    }

    private void DisconnectWithException(Exception e)
    {
        DisconnectWithException(this, e);
    }
    private void DisconnectWithException(object? sender, Exception? e)
    {
        _logger.LogError(e, "Disconnecting peer {peer}", PeerAddress.PubKey);
        _cancellationTokenSource.Cancel();
        _messageService.Dispose();

        DisconnectEvent?.Invoke(sender, EventArgs.Empty);
    }

    private void HandleMessage(object? sender, IMessage? message)
    {
        if (message is null)
        {
            return;
        }

        if (!_isInitialized)
        {
            _logger.LogTrace("[Peer] Received message from peer {peer} but was not initialized", PeerAddress.PubKey);
            HandleInitialization(message);
        }
        else
        {
            switch (message.Type)
            {
                case MessageTypes.PING:
                    _logger.LogTrace("[Peer] Received ping message from peer {peer}", PeerAddress.PubKey);
                    _ = HandlePingAsync(message);
                    break;
                case MessageTypes.PONG:
                    _logger.LogTrace("[Peer] Received pong message from peer {peer}", PeerAddress.PubKey);
                    _pingPongService.HandlePong(message);
                    break;
            }
        }
    }

    private void HandleException(object? sender, Exception e)
    {
        DisconnectWithException(sender, e);
    }

    private void HandleInitialization(IMessage message)
    {
        // Check if first message is an init message
        if (message.Type != MessageTypes.INIT || message is not InitMessage initMessage)
        {
            DisconnectWithException(new ConnectionException("Failed to receive init message"));
            return;
        }

        // Check if Features are compatible
        if (!_nodeOptions.GetNodeFeatures().IsCompatible(initMessage.Payload.FeatureSet))
        {
            DisconnectWithException(new ConnectionException("Peer is not compatible"));
            return;
        }

        // Check if Chains are compatible
        if (initMessage.Extension != null && initMessage.Extension.TryGetTlv(TlvConstants.NETWORKS, out var networksTlv))
        {
            // Check if ChainHash contained in networksTlv.ChainHashes exists in our ChainHashes
            var networkChainHashes = ((NetworksTlv)networksTlv!).ChainHashes;
            if (networkChainHashes != null)
            {
                if (networkChainHashes.Any(chainHash => !_nodeOptions.ChainHashes.Contains(chainHash)))
                {
                    DisconnectWithException(new ConnectionException("Peer chain is not compatible"));
                    return;
                }
            }
        }

        NodeOptions.GetNodeOptions(initMessage.Payload.FeatureSet, initMessage.Extension);

        _logger.LogTrace("[Peer] Message from peer {peer} is correct (init)", PeerAddress.PubKey);

        StartPingPongService();

        _isInitialized = true;
    }

    private void StartPingPongService()
    {
        _pingPongService.PingMessageReadyEvent += (sender, pingMessage) =>
        {
            // We can only send ping messages if the peer is initialized
            if (!_isInitialized)
            {
                return;
            }

            _ = _messageService.SendMessageAsync(pingMessage, _cancellationTokenSource.Token).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    DisconnectWithException(new ConnectionException("Failed to send ping message", task.Exception));
                }
            });
        };

        // Setup Ping to keep connection alive
        _ = _pingPongService.StartPingAsync(_cancellationTokenSource.Token).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                DisconnectWithException(new ConnectionException("Failed to start ping service", task.Exception));
            }
        });

        _logger.LogInformation("[Peer] Ping service started for peer {peer}", PeerAddress.PubKey);
    }

    private async Task HandlePingAsync(IMessage pingMessage)
    {
        var pongMessage = _messageFactory.CreatePongMessage(pingMessage);
        await _messageService.SendMessageAsync(pongMessage);
    }

    public void Disconnect()
    {
        _logger.LogInformation("Disconnecting peer {peer}", PeerAddress.PubKey);
        _cancellationTokenSource.Cancel();
        _messageService.Dispose();

        DisconnectEvent?.Invoke(this, EventArgs.Empty);
    }
}