using Microsoft.Extensions.Logging;

namespace NLightning.Application.Node.Services;

using Interfaces;
using Domain.Channels.Interfaces;
using Domain.Crypto.ValueObjects;
using Domain.Exceptions;
using Domain.Node.Options;
using Domain.Protocol.Constants;
using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Tlv;

/// <summary>
/// Application service for peer communication that orchestrates domain logic.
/// </summary>
public sealed class PeerApplicationService : IPeerService
{
    private readonly IChannelManager _channelManager;
    private readonly IPeerCommunicationService _communicationService;
    private readonly ILogger<PeerApplicationService> _logger;

    private FeatureOptions _features;
    private bool _isInitialized;

    /// <summary>
    /// Event raised when the peer is disconnected.
    /// </summary>
    public event EventHandler? DisconnectEvent;

    /// <summary>
    /// Gets the peer's public key.
    /// </summary>
    public CompactPubKey PeerPubKey => _communicationService.PeerCompactPubKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="PeerApplicationService"/> class.
    /// </summary>
    /// <param name="channelManager">The channel manager</param>
    /// <param name="communicationService">The peer communication service</param>
    /// <param name="features">The feature options</param>
    /// <param name="logger">A logger</param>
    /// <param name="networkTimeout">Network timeout</param>
    public PeerApplicationService(IChannelManager channelManager, IPeerCommunicationService communicationService,
                                  FeatureOptions features, ILogger<PeerApplicationService> logger,
                                  TimeSpan networkTimeout)
    {
        _channelManager = channelManager;
        _communicationService = communicationService;
        _features = features;
        _logger = logger;

        // Set up event handlers
        _communicationService.MessageReceived += HandleMessage;
        _communicationService.ExceptionRaised += HandleException;
        _communicationService.DisconnectEvent += (_, _) => DisconnectEvent?.Invoke(this, EventArgs.Empty);

        // Initialize communication
        _communicationService.InitializeAsync(networkTimeout).Wait();
    }

    /// <summary>
    /// Handles messages received from the peer.
    /// </summary>
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
        else if (message is IChannelMessage channelMessage)
        {
            // Handle channel-related messages
            _ = HandleChannelMessageAsync(channelMessage);
        }
    }

    /// <summary>
    /// Handles exceptions raised by the communication service.
    /// </summary>
    private void HandleException(object? sender, Exception e)
    {
        _logger.LogError(e, "Exception occurred with peer {peer}", PeerPubKey);
    }

    /// <summary>
    /// Handles the initialization process when receiving the first message.
    /// </summary>
    private void HandleInitialization(IMessage message)
    {
        // Check if the first message is an init message
        if (message.Type != MessageTypes.Init || message is not InitMessage initMessage)
        {
            _logger.LogError("Failed to receive init message from peer {peer}", PeerPubKey);
            Disconnect();
            return;
        }

        // Check if Features are compatible
        if (!_features.GetNodeFeatures().IsCompatible(initMessage.Payload.FeatureSet, out var negotiatedFeatures)
         || negotiatedFeatures is null)
        {
            _logger.LogError("Peer {peer} is not compatible", PeerPubKey);
            Disconnect();
            return;
        }

        // Check if Chains are compatible
        if (initMessage.Extension != null
         && initMessage.Extension.TryGetTlv(TlvConstants.Networks, out var networksTlv))
        {
            // Check if ChainHash contained in networksTlv.ChainHashes exists in our ChainHashes
            var networkChainHashes = ((NetworksTlv)networksTlv!).ChainHashes;
            if (networkChainHashes is not null &&
                networkChainHashes.Any(chainHash => !_features.ChainHashes.Contains(chainHash)))
            {
                _logger.LogError("Peer {peer} chain is not compatible", PeerPubKey);
                Disconnect();
                return;
            }
        }

        _features = FeatureOptions.GetNodeOptions(negotiatedFeatures, initMessage.Extension);
        _logger.LogTrace("Initialization from peer {peer} completed successfully", PeerPubKey);
        _isInitialized = true;
    }

    /// <summary>
    /// Handles channel messages.
    /// </summary>
    private async Task HandleChannelMessageAsync(IChannelMessage message)
    {
        try
        {
            _logger.LogTrace("Received channel message ({messageType}) from peer {peer}",
                             Enum.GetName(message.Type), PeerPubKey);

            var replyMessage = _channelManager.HandleChannelMessage(message, _features, PeerPubKey);
            await _communicationService.SendMessageAsync(replyMessage);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error handling channel message ({messageType}) from peer {peer}",
                             Enum.GetName(message.Type), PeerPubKey);

            if (e is ChannelErrorException channelError)
            {
                _logger.LogError("Channel error: {message}",
                                 !string.IsNullOrEmpty(channelError.PeerMessage)
                                     ? channelError.PeerMessage
                                     : channelError.Message);
            }

            Disconnect();
        }
    }

    /// <summary>
    /// Disconnects from the peer.
    /// </summary>
    public void Disconnect()
    {
        _logger.LogInformation("Disconnecting peer {peer}", PeerPubKey);
        _communicationService.Disconnect();
    }
}