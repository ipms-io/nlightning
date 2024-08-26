namespace NLightning.Bolts.BOLT1.Primitives;

using Bolts.Factories;
using Bolts.Interfaces;
using Common.Constants;
using Common.TLVs;
using Constants;
using Exceptions;
using Interfaces;
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
    private readonly NodeOptions _nodeOptions;
    private readonly bool _isInbound;

    private bool _isInitialized;

    /// <summary>
    /// Event raised when the peer is disconnected.
    /// </summary>
    public event EventHandler? DisconnectEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="Peer"/> class.
    /// </summary>
    /// <param name="nodeOptions">The node options.</param>
    /// <param name="messageService">The message service.</param>
    /// <param name="pingPongService">The ping pong service.</param>
    /// <param name="isInbound">A value indicating whether the peer is inbound.</param>
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    internal Peer(NodeOptions nodeOptions, IMessageService messageService, IPingPongService pingPongService, bool isInbound)
    {
        _messageService = messageService;
        _pingPongService = pingPongService;
        _nodeOptions = nodeOptions;
        _isInbound = isInbound;

        _messageService.MessageReceived += HandleMessageAsync;

        // Send the init message if we're the outbound peer
        if (!_isInbound)
        {
            var initMessage = MessageFactory.CreateInitMessage(_nodeOptions);
            _messageService.SendMessageAsync(initMessage, _cancellationTokenSource.Token).Wait();
        }
        else
        {
            // Set timeout to close connection if the other peer doesn't send an init message
            Task.Delay(_nodeOptions.NetworkTimeout, _cancellationTokenSource.Token).ContinueWith(_ =>
            {
                if (!_isInitialized)
                {
                    Disconnect();
                }
            });
        }

        if (!_messageService.IsConnected)
        {
            throw new ConnectionException("Failed to connect to peer");
        }
    }

    private void Disconnect()
    {
        _cancellationTokenSource.Cancel();
        _messageService.Dispose();

        DisconnectEvent?.Invoke(this, EventArgs.Empty);
    }

    private void HandleMessageAsync(object? sender, IMessage message)
    {
        if (!_isInitialized)
        {
            HandleInitializationAsync(message);
            if (!_isInbound)
            {
                return;
            }

            var initMessage = MessageFactory.CreateInitMessage(_nodeOptions);
            _messageService.SendMessageAsync(initMessage, _cancellationTokenSource.Token);
        }
        else
        {
            switch (message.Type)
            {
                case MessageTypes.PING:
                    _ = HandlePingAsync((PingMessage)message);
                    break;
                case MessageTypes.PONG:
                    _pingPongService.HandlePong((PongMessage)message);
                    break;
            }
        }
    }

    private void HandleInitializationAsync(IMessage message)
    {
        // Check if first message is an init message
        if (message.Type != MessageTypes.INIT || message is not InitMessage initMessage)
        {
            Disconnect();

            throw new ConnectionException("Failed to receive init message");
        }

        // Check if Features are compatible
        var isCompatible = _nodeOptions.GetNodeFeatures().IsCompatible(initMessage.Payload.Features);
        if (isCompatible == false)
        {
            Disconnect();

            throw new ConnectionException("Peer is not compatible");
        }

        // Check if Chains are compatible
        if (initMessage.Extension != null && initMessage.Extension.TryGetTlv(TlvConstants.NETWORKS, out var networksTlv))
        {
            // Check if if ChainHash contained in networksTlv.ChainHashes exists in our ChainHashes
            var networkChainHashes = ((NetworksTlv)networksTlv!).ChainHashes;
            if (networkChainHashes != null)
            {
                foreach (var chainHash in networkChainHashes)
                {
                    if (!_nodeOptions.ChainHashes.Contains(chainHash))
                    {
                        Disconnect();

                        throw new ConnectionException("Peer chain is not compatible");
                    }
                }
            }
        }

        _isInitialized = true;

        // Setup Ping to keep connection alive
        Task.Run(() => _pingPongService.StartPingAsync(_cancellationTokenSource.Token));
    }

    private async Task HandlePingAsync(PingMessage pingMessage)
    {
        var pongMessage = MessageFactory.CreatePongMessage(pingMessage.Payload.NumPongBytes);
        await _messageService.SendMessageAsync(pongMessage);
    }
}