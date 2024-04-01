using NLightning.Bolts.BOLT1.Interfaces;
using NLightning.Bolts.BOLT1.Messages;
using NLightning.Bolts.Factories;
using NLightning.Bolts.Interfaces;

namespace NLightning.Bolts.BOLT1.Primitives;

public sealed class Peer
{
    private readonly IMessageService _messageService;
    private readonly PeerAddress _peerAddress;
    private readonly NodeOptions _nodeOptions;

    private bool _isInitialized;

    public event EventHandler? Disconect;

    internal Peer(NodeOptions nodeOptions, IMessageService messageService, PeerAddress peerAddress)
    {
        _messageService = messageService;
        _peerAddress = peerAddress;
        _nodeOptions = nodeOptions;

        _messageService.MessageReceived += HandleMessageAsync;

        // Send the init message
        var initMessage = MessageFactory.CreateInitMessage(_nodeOptions);
        _messageService.SendMessageAsync(initMessage).Wait();

        if (_messageService.IsConnected == false)
        {
            throw new Exception("Failed to connect to peer");
        }
    }

    private void HandleMessageAsync(object? sender, IMessage message)
    {
        if (!_isInitialized)
        {
            HandleInitializationAsync(message);
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