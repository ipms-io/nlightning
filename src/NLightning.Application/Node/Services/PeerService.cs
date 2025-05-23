using Microsoft.Extensions.Logging;
using NBitcoin;
using NLightning.Application.Node.Services.Interfaces;
using NLightning.Application.Protocol.Factories;
using NLightning.Domain.Exceptions;
using NLightning.Domain.Node.Options;
using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Managers;
using NLightning.Domain.Protocol.Messages;
using NLightning.Domain.Protocol.Messages.Interfaces;
using NLightning.Domain.Protocol.Payloads;
using NLightning.Domain.Protocol.Services;
using NLightning.Domain.Protocol.Tlv;
using NLightning.Domain.ValueObjects;

namespace NLightning.Application.Node.Services;

/// <summary>
/// Represents a peer in the network.
/// </summary>
/// <remarks>
/// This class is used to communicate with a peer in the network.
/// </remarks>
public sealed class PeerService : IPeerService
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IChannelManager _channelManager;
    private readonly ILogger<PeerService> _logger;
    private readonly IMessageFactory _messageFactory;
    private readonly IMessageService _messageService;
    private readonly IPingPongService _pingPongService;

    private FeatureOptions _features;
    private bool _isInitialized;

    /// <summary>
    /// Event raised when the peer is disconnected.
    /// </summary>
    public event EventHandler? DisconnectEvent;

    public PubKey PeerPubKey { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PeerService"/> class.
    /// </summary>
    /// <param name="channelManager">The channel manager</param>
    /// <param name="features">The feature options</param>
    /// <param name="logger">A logger</param>
    /// <param name="messageFactory">The message factory</param>
    /// <param name="messageService">The message service.</param>
    /// <param name="networkTimeout">Network timeout</param>
    /// <param name="peerPubKey">Peer pubkey</param>
    /// <param name="pingPongService">The ping pong service.</param>
    /// <exception cref="ConnectionException">Thrown when the connection to the peer fails.</exception>
    internal PeerService(IChannelManager channelManager, FeatureOptions features, ILogger<PeerService> logger,
                         IMessageFactory messageFactory, IMessageService messageService, TimeSpan networkTimeout,
                         PubKey peerPubKey, IPingPongService pingPongService)
    {
        _channelManager = channelManager;
        _features = features;
        _logger = logger;
        _messageFactory = messageFactory;
        _messageService = messageService;
        _pingPongService = pingPongService;

        PeerPubKey = peerPubKey;

        _messageService.MessageReceived += HandleMessage;
        _messageService.ExceptionRaised += HandleException;
        _pingPongService.DisconnectEvent += HandleException;

        // Always send an init message upon connection
        logger.LogTrace("Sending init message to peer {peer}", peerPubKey);
        var initMessage = _messageFactory.CreateInitMessage();
        _messageService.SendMessageAsync(initMessage, _cancellationTokenSource.Token).Wait();

        // Wait for an init message
        logger.LogTrace("Waiting for init message from peer {peer}", peerPubKey);
        // Set timeout to close connection if the other peer doesn't send an init message
        Task.Delay(networkTimeout, _cancellationTokenSource.Token).ContinueWith(task =>
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
        if (e is ErrorException)
        {
            ChannelId? channelId = null;
            var message = e.Message;
            if (e is ChannelErrorException channelErrorException)
            {
                channelId = channelErrorException.ChannelId;
                if (!string.IsNullOrWhiteSpace(channelErrorException.PeerMessage))
                    message = channelErrorException.PeerMessage;
            }
            _messageService.SendMessageAsync(new ErrorMessage(new ErrorPayload(channelId, message)));
        }

        _logger.LogError(e, "Disconnecting peer {peer}", PeerPubKey);
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
            _logger.LogTrace("Received message from peer {peer} but was not initialized", PeerPubKey);
            HandleInitialization(message);
        }
        else
        {
            switch (message.Type)
            {
                case MessageTypes.Ping:
                    _logger.LogTrace("Received ping message from peer {peer}", PeerPubKey);
                    _ = HandlePingAsync(message);
                    break;
                case MessageTypes.Pong:
                    _logger.LogTrace("Received pong message from peer {peer}", PeerPubKey);
                    _pingPongService.HandlePong(message);
                    break;
                case MessageTypes.OpenChannel:
                case MessageTypes.AcceptChannel:
                case MessageTypes.FundingCreated:
                case MessageTypes.FundingSigned:
                case MessageTypes.OpenChannel2:
                case MessageTypes.AcceptChannel2:
                case MessageTypes.TxAddInput:
                case MessageTypes.TxAddOutput:
                case MessageTypes.TxRemoveInput:
                case MessageTypes.TxRemoveOutput:
                case MessageTypes.TxComplete:
                case MessageTypes.TxSignatures:
                case MessageTypes.TxInitRbf:
                case MessageTypes.TxAckRbf:
                case MessageTypes.TxAbort:
                    if (message is IChannelMessage channelMessage)
                    {
                        _ = HandleChannelMessageAsync(channelMessage);
                    }
                    else
                    {
                        DisconnectWithException(new ChannelErrorException(
                            $"Message from peer {PeerPubKey} can't be boxed to IChannelMessage"));
                    }
                    break;
            }
        }
    }

    private void HandleException(object? sender, Exception e)
    {
        if (e is WarningException)
        {
            ChannelId? channelId = null;
            var message = e.Message;
            if (e is ChannelWarningException channelWarningException)
            {
                channelId = channelWarningException.ChannelId;
                if (!string.IsNullOrWhiteSpace(channelWarningException.PeerMessage))
                    message = channelWarningException.PeerMessage;
            }
            _messageService.SendMessageAsync(new WarningMessage(new ErrorPayload(channelId, message)));
        }

        DisconnectWithException(sender, e);
    }

    private void HandleInitialization(IMessage message)
    {
        // Check if the first message is an init message
        if (message.Type != MessageTypes.Init || message is not InitMessage initMessage)
        {
            DisconnectWithException(new ConnectionException("Failed to receive init message"));
            return;
        }

        // Check if Features are compatible
        if (!_features.GetNodeFeatures().IsCompatible(initMessage.Payload.FeatureSet, out var negotiatedFeatures)
            || negotiatedFeatures is null)
        {
            DisconnectWithException(new ConnectionException("Peer is not compatible"));
            return;
        }

        // Check if Chains are compatible
        if (initMessage.Extension != null
            && initMessage.Extension.TryGetTlv(TlvConstants.Networks, out var networksTlv))
        {
            // Check if ChainHash contained in networksTlv.ChainHashes exists in our ChainHashes
            var networkChainHashes = ((NetworksTlv)networksTlv!).ChainHashes;
            if (networkChainHashes != null)
            {
                if (networkChainHashes.Any(chainHash => !_features.ChainHashes.Contains(chainHash)))
                {
                    DisconnectWithException(new ConnectionException("Peer chain is not compatible"));
                    return;
                }
            }
        }

        _features = FeatureOptions.GetNodeOptions(negotiatedFeatures, initMessage.Extension);

        _logger.LogTrace("Message from peer {peer} is correct (init)", PeerPubKey);

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

        _logger.LogInformation("Ping service started for peer {peer}", PeerPubKey);
    }

    private async Task HandlePingAsync(IMessage pingMessage)
    {
        var pongMessage = _messageFactory.CreatePongMessage(pingMessage);
        await _messageService.SendMessageAsync(pongMessage);
    }

    private async Task HandleChannelMessageAsync(IChannelMessage message)
    {
        try
        {
            _logger.LogTrace("Received channel message ({messageType}) from peer {peer}",
                             Enum.GetName(message.Type), PeerPubKey);

            var replyMessage = _channelManager.HandleChannelMessage(message, _features, PeerPubKey);
            // if (replyMessage is not null) 
            await _messageService.SendMessageAsync(replyMessage, _cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error handling channel message ({messageType}) from peer {peer}",
                             Enum.GetName(message.Type), PeerPubKey);

            HandleException(this, e);
        }
    }

    public void Disconnect()
    {
        _logger.LogInformation("Disconnecting peer {peer}", PeerPubKey);
        _cancellationTokenSource.Cancel();
        _messageService.Dispose();

        DisconnectEvent?.Invoke(this, EventArgs.Empty);
    }
}