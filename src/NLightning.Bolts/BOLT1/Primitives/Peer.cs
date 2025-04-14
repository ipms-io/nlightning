namespace NLightning.Bolts.BOLT1.Primitives;

using Bolts.Factories;
using Common.Constants;
using Common.Interfaces;
using Common.Managers;
using Common.Node;
using Common.TLVs;
using Messages;

/// <summary>
/// Represents a peer in the network.
/// </summary>
/// <remarks>
/// This class is used to communicate with a peer in the network.
/// </remarks>
internal sealed class Peer
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IMessageService _messageService;
    private readonly IPingPongService _pingPongService;
    private readonly ILogger _logger;
    private readonly PeerAddress _peerAddress;
    private readonly bool _isInbound;

    private bool _isInitialized;

    /// <summary>
    /// Event raised when the peer is disconnected.
    /// </summary>
    public event EventHandler<Exception?>? DisconnectEvent;

    public NodeOptions? NodeOptions { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Peer"/> class.
    /// </summary>
    /// <param name="messageService">The message service.</param>
    /// <param name="pingPongService">The ping pong service.</param>
    /// <param name="isInbound">A value indicating whether the peer is inbound.</param>
    /// <param name="logger">A logger implementing <see cref="ILogger">ILogger</see>/></param>
    /// <param name="peerAddress">Peer address</param>
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    internal Peer(IMessageService messageService, IPingPongService pingPongService, ILogger logger,
                  PeerAddress peerAddress, bool isInbound)
    {
        _messageService = messageService;
        _pingPongService = pingPongService;
        _peerAddress = peerAddress;
        _logger = logger;
        _isInbound = isInbound;

        _messageService.MessageReceived += HandleMessage;
        _messageService.ExceptionRaised += HandleException;
        _pingPongService.DisconnectEvent += HandleException;

        // Send the init message if we're the outbound peer
        if (!_isInbound)
        {
            logger.Verbose("[Peer] Sending init message to peer {peer}", _peerAddress.PubKey);
            var initMessage = MessageFactory.CreateInitMessage(ConfigManager.NodeOptions);
            _messageService.SendMessageAsync(initMessage, _cancellationTokenSource.Token).Wait();
        }
        else
        {
            logger.Verbose("[Peer] Waiting init message from peer {peer}", _peerAddress.PubKey);
            // Set timeout to close connection if the other peer doesn't send an init message
            Task.Delay(ConfigManager.Instance.NetworkTimeout, _cancellationTokenSource.Token).ContinueWith(task =>
            {
                if (!task.IsCanceled && !_isInitialized)
                {
                    Disconnect(new ConnectionException("Peer did not send init message after timeout"));
                }
            });
        }

        if (!_messageService.IsConnected)
        {
            throw new ConnectionException("Failed to connect to peer");
        }
    }

    private void Disconnect(Exception e)
    {
        Disconnect(this, e);
    }
    private void Disconnect(object? sender, Exception? e)
    {
        _cancellationTokenSource.Cancel();
        _messageService.Dispose();

        DisconnectEvent?.Invoke(sender, e);
    }

    private void HandleMessage(object? sender, IMessage? message)
    {
        if (message is null)
        {
            return;
        }

        if (!_isInitialized)
        {
            _logger.Verbose("[Peer] Received message from peer {peer} but was not initialized", _peerAddress.PubKey);
            HandleInitialization(message);
        }
        else
        {
            switch (message.Type)
            {
                case MessageTypes.PING:
                    _logger.Verbose("[Peer] Received ping message from peer {peer}", _peerAddress.PubKey);
                    _ = HandlePingAsync((PingMessage)message);
                    break;
                case MessageTypes.PONG:
                    _logger.Verbose("[Peer] Received pong message from peer {peer}", _peerAddress.PubKey);
                    _pingPongService.HandlePong((PongMessage)message);
                    break;
            }
        }
    }

    private void HandleException(object? sender, Exception e)
    {
        Disconnect(sender, e);
    }

    private void HandleInitialization(IMessage message)
    {
        // Check if first message is an init message
        if (message.Type != MessageTypes.INIT || message is not InitMessage initMessage)
        {
            Disconnect(new ConnectionException("Failed to receive init message"));
            return;
        }

        // Check if Features are compatible
        var isCompatible = ConfigManager.NodeOptions.GetNodeFeatures().IsCompatible(initMessage.Payload.FeatureSet);
        if (isCompatible == false)
        {
            Disconnect(new ConnectionException("Peer is not compatible"));
            return;
        }

        // Check if Chains are compatible
        if (initMessage.Extension != null && initMessage.Extension.TryGetTlv(TlvConstants.NETWORKS, out var networksTlv))
        {
            // Check if ChainHash contained in networksTlv.ChainHashes exists in our ChainHashes
            var networkChainHashes = ((NetworksTlv)networksTlv!).ChainHashes;
            if (networkChainHashes != null)
            {
                if (networkChainHashes.Any(chainHash => !ConfigManager.NodeOptions.ChainHashes.Contains(chainHash)))
                {
                    Disconnect(new ConnectionException("Peer chain is not compatible"));
                    return;
                }
            }
        }

        NodeOptions = NodeOptions.GetNodeOptions(initMessage.Payload.FeatureSet, initMessage.Extension);

        _logger.Verbose("[Peer] Message from peer {peer} is correct (init)", _peerAddress.PubKey);

        if (_isInbound)
        {
            _logger.Verbose("[Peer] Sending init message to peer {peer}", _peerAddress.PubKey);
            var ourInitMessage = MessageFactory.CreateInitMessage(ConfigManager.NodeOptions);
            _ = _messageService.SendMessageAsync(ourInitMessage, _cancellationTokenSource.Token);
        }
        else
        {
            StartPingPongService();
        }

        _isInitialized = true;
    }

    private void StartPingPongService()
    {
        _pingPongService.PingMessageReadyEvent += (sender, pingMessage) =>
        {
            _ = _messageService.SendMessageAsync(pingMessage, _cancellationTokenSource.Token).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Disconnect(new ConnectionException("Failed to send ping message", task.Exception));
                }
            });
        };

        // Setup Ping to keep connection alive
        _ = _pingPongService.StartPingAsync(_cancellationTokenSource.Token).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Disconnect(new ConnectionException("Failed to start ping service", task.Exception));
            }
        });

        _logger.Information("[Peer] Ping service started for peer {peer}", _peerAddress.PubKey);
    }

    private async Task HandlePingAsync(PingMessage pingMessage)
    {
        var pongMessage = MessageFactory.CreatePongMessage(pingMessage.Payload.NumPongBytes);
        await _messageService.SendMessageAsync(pongMessage);
    }
}