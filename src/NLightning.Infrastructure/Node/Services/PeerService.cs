using System.Net;
using Microsoft.Extensions.Logging;

namespace NLightning.Infrastructure.Node.Services;

using Domain.Crypto.ValueObjects;
using Domain.Node.Events;
using Domain.Node.Interfaces;
using Domain.Node.Options;
using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;

// TODO: Eventually move this to the Application layer
/// <summary>
/// Service for peer communication
/// </summary>
public sealed class PeerService : IPeerService
{
    private readonly IPeerCommunicationService _peerCommunicationService;
    private readonly ILogger<PeerService> _logger;

    private bool _isInitialized;

    /// <inheritdoc/>
    public event EventHandler<PeerDisconnectedEventArgs>? OnDisconnect;

    /// <inheritdoc/>
    public event EventHandler<ChannelMessageEventArgs>? OnChannelMessageReceived;

    /// <inheritdoc/>
    public CompactPubKey PeerPubKey => _peerCommunicationService.PeerCompactPubKey;

    public string? PreferredHost { get; private set; }
    public ushort? PreferredPort { get; private set; }

    public FeatureOptions Features { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PeerService"/> class.
    /// </summary>
    /// <param name="peerCommunicationService">The peer communication service</param>
    /// <param name="features">The feature options</param>
    /// <param name="logger">A logger</param>
    /// <param name="networkTimeout">Network timeout</param>
    public PeerService(IPeerCommunicationService peerCommunicationService, FeatureOptions features,
                       ILogger<PeerService> logger, TimeSpan networkTimeout)
    {
        _peerCommunicationService = peerCommunicationService;
        Features = features;
        _logger = logger;

        // Set up event handlers
        _peerCommunicationService.MessageReceived += HandleMessage;
        _peerCommunicationService.ExceptionRaised += HandleException;
        _peerCommunicationService.DisconnectEvent += HandleDisconnection;

        // Initialize communication
        _peerCommunicationService.InitializeAsync(networkTimeout).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Disconnects from the peer.
    /// </summary>
    public void Disconnect()
    {
        _logger.LogInformation("Disconnecting peer {peer}", PeerPubKey);
        _peerCommunicationService.Disconnect();
    }

    public Task SendMessageAsync(IChannelMessage replyMessage)
    {
        return _peerCommunicationService.SendMessageAsync(replyMessage);
    }

    /// <summary>
    /// Handles messages received from the peer.
    /// </summary>
    private void HandleMessage(object? sender, IMessage? message)
    {
        if (message is null)
            return;

        if (!_isInitialized)
        {
            _logger.LogTrace("Received message from peer {peer} but was not initialized", PeerPubKey);
            HandleInitialization(message);
        }
        else if (message is IChannelMessage channelMessage)
        {
            // Handle channel-related messages
            HandleChannelMessage(channelMessage);
        }
    }

    /// <summary>
    /// Handles exceptions raised by the communication service.
    /// </summary>
    private void HandleException(object? sender, Exception e)
    {
        _logger.LogError(e, "Exception occurred with peer {peer}", PeerPubKey);
    }

    private void HandleDisconnection(object? sender, EventArgs e)
    {
        OnDisconnect?.Invoke(this, new PeerDisconnectedEventArgs(PeerPubKey));
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
        if (!Features.GetNodeFeatures().IsCompatible(initMessage.Payload.FeatureSet, out var negotiatedFeatures)
         || negotiatedFeatures is null)
        {
            _logger.LogError("Peer {peer} is not compatible", PeerPubKey);
            Disconnect();
            return;
        }

        // Check if ChainHash contained in networksTlv.ChainHashes exists in our ChainHashes
        var networkChainHashes = initMessage.NetworksTlv?.ChainHashes;
        if (networkChainHashes != null
         && networkChainHashes.Any(chainHash => !Features.ChainHashes.Contains(chainHash)))
        {
            _logger.LogError("Peer {peer} chain is not compatible", PeerPubKey);
            Disconnect();
            return;
        }

        if (initMessage.RemoteAddressTlv is not null)
        {
            switch (initMessage.RemoteAddressTlv.AddressType)
            {
                case 1 or 2:
                    {
                        if (!IPAddress.TryParse(initMessage.RemoteAddressTlv.Address, out var ipAddress))
                        {
                            _logger.LogWarning("Peer {peer} has an invalid remote address: {address}",
                                               PeerPubKey, initMessage.RemoteAddressTlv.Address);
                        }
                        else
                        {
                            PreferredHost = ipAddress.ToString();
                            PreferredPort = initMessage.RemoteAddressTlv.Port;
                        }

                        break;
                    }
                case 5:
                    PreferredHost = initMessage.RemoteAddressTlv.Address;
                    PreferredPort = initMessage.RemoteAddressTlv.Port;
                    break;
                default:
                    _logger.LogWarning("Peer {peer} has an unsupported remote address type: {addressType}",
                                       PeerPubKey, initMessage.RemoteAddressTlv.AddressType);
                    break;
            }
        }

        Features = FeatureOptions.GetNodeOptions(negotiatedFeatures, initMessage.Extension);
        _logger.LogTrace("Initialization from peer {peer} completed successfully", PeerPubKey);
        _isInitialized = true;
    }

    /// <summary>
    /// Handles channel messages.
    /// </summary>
    private void HandleChannelMessage(IChannelMessage message)
    {
        _logger.LogTrace("Received channel message ({messageType}) from peer {peer}",
                         Enum.GetName(message.Type), PeerPubKey);

        OnChannelMessageReceived?.Invoke(this, new ChannelMessageEventArgs(message, PeerPubKey));
    }
}