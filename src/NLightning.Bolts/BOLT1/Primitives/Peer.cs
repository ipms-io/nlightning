namespace NLightning.Bolts.BOLT1.Primitives;

using Bolts.Interfaces;
using Factories;
using Interfaces;
using Messages;

public sealed class Peer
{
    private readonly IMessageService _messageService;
    private readonly PeerAddress _peerAddress;
    private readonly NodeOptions _nodeOptions;
    private readonly bool _isInbound;

    private bool _isInitialized;

    public event EventHandler? Disconect;

    internal Peer(NodeOptions nodeOptions, IMessageService messageService, PeerAddress peerAddress, bool isInbound)
    {
        _messageService = messageService;
        _peerAddress = peerAddress;
        _nodeOptions = nodeOptions;
        _isInbound = isInbound;

        _messageService.MessageReceived += HandleMessageAsync;

        // Send the init message if we're the outbound peer
        if (!_isInbound)
        {
            var initMessage = MessageFactory.CreateInitMessage(_nodeOptions);
            _messageService.SendMessageAsync(initMessage).Wait();
        }

        if (!_messageService.IsConnected)
        {
            throw new Exception("Failed to connect to peer");
        }
    }

    private void HandleMessageAsync(object? sender, IMessage message)
    {
        if (!_isInitialized)
        {
            HandleInitializationAsync(message);
            if (_isInbound)
            {
                var initMessage = MessageFactory.CreateInitMessage(_nodeOptions);
                _messageService.SendMessageAsync(initMessage);
            }
        }
    }

    private void HandleInitializationAsync(IMessage message)
    {
        if (message.Type != 16 || message is not InitMessage initMessage)
        {
            _messageService.Dispose();

            Disconect?.Invoke(this, EventArgs.Empty);

            throw new Exception("Failed to receive init message");
        }

        var isCompatible = _nodeOptions.GetNodeFeatures().IsCompatible(initMessage.Payload.Features);

        if (isCompatible == false)
        {
            _messageService.Dispose();

            Disconect?.Invoke(this, EventArgs.Empty);

            throw new Exception("Peer is not compatible");
        }

        _isInitialized = true;
    }
}