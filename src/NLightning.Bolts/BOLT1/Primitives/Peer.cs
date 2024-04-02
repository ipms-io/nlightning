namespace NLightning.Bolts.BOLT1.Primitives;

using Bolts.Interfaces;
using Common.Constants;
using Common.TLVs;
using Constants;
using Factories;
using Interfaces;
using Messages;

public sealed class Peer
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IMessageService _messageService;
    private readonly IPingPongService _pingPongService;
    private readonly PeerAddress _peerAddress;
    private readonly NodeOptions _nodeOptions;
    private readonly bool _isInbound;

    private bool _isInitialized;

    public event EventHandler? DisconnectEvent;

    internal Peer(NodeOptions nodeOptions, IMessageService messageService, IPingPongService pingPongService, PeerAddress peerAddress, bool isInbound)
    {
        _messageService = messageService;
        _pingPongService = pingPongService;
        _peerAddress = peerAddress;
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
            throw new Exception("Failed to connect to peer");
        }
    }

    private void Disconnect()
    {
        _cancellationTokenSource.Cancel();
        _messageService.Dispose();
    }

    private void HandleMessageAsync(object? sender, IMessage message)
    {
        if (!_isInitialized)
        {
            HandleInitializationAsync(message);
            if (_isInbound)
            {
                var initMessage = MessageFactory.CreateInitMessage(_nodeOptions);
                _messageService.SendMessageAsync(initMessage, _cancellationTokenSource.Token);
            }
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
                default:
                    // Handle other messages
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

            throw new Exception("Failed to receive init message");
        }

        // Check if Features are compatible
        var isCompatible = _nodeOptions.GetNodeFeatures().IsCompatible(initMessage.Payload.Features);
        if (isCompatible == false)
        {
            Disconnect();

            throw new Exception("Peer is not compatible");
        }

        // Check if Chains are compatible
        if (initMessage.Extension != null && initMessage.Extension.TryGetTlv(TLVConstants.NETWORKS, out var networksTlv))
        {
            // Check if if ChainHash contained in networksTlv.ChainHashes exists in our ChainHashes
            var networkChainHashes = ((NetworksTLV)networksTlv!).ChainHashes;
            if (networkChainHashes != null)
            {
                foreach (var chainHash in networkChainHashes)
                {
                    if (!_nodeOptions.ChainHashes.Contains(chainHash))
                    {
                        Disconnect();

                        throw new Exception("Peer is not compatible");
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