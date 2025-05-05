using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace NLightning.Bolts.BOLT2.Managers;

using Common.Channels;
using Common.Constants;
using Common.Enums;
using Common.Interfaces;
using Common.Messages;
using Common.Models;
using Common.Options;

public class ChannelManager : IChannelManager
{
    private readonly Dictionary<ChannelId, Channel> _channels = [];
    private readonly Dictionary<ChannelId, ChannelState> _channelStates = [];
    private readonly Dictionary<ChannelId, IChannelMessage> _channelLastReceivedMessage = [];
    private readonly Dictionary<(PubKey, ChannelId), Channel> _temporaryChannels = [];
    private readonly Dictionary<(PubKey, ChannelId), ChannelState> _temporaryChannelStates = [];
    private readonly Dictionary<(PubKey, ChannelId), IChannelMessage> _temporaryChannelLastReceivedMessage = [];

    private readonly IChannelFactory _channelFactory;
    private readonly ILogger<ChannelManager> _logger;
    private readonly IMessageFactory _messageFactory;
    private readonly NodeOptions _nodeOptions;

    public ChannelManager(IChannelFactory channelFactory, ILogger<ChannelManager> logger,
                          IMessageFactory messageFactory, IOptions<NodeOptions> nodeOptions)
    {
        _channelFactory = channelFactory;
        _logger = logger;
        _messageFactory = messageFactory;
        _nodeOptions = nodeOptions.Value;
    }

    public IChannelMessage? HandleChannelMessage(IChannelMessage message, FeatureOptions negotiatedFeatures)
    {
        // Check if channel exists on the state dictionary
        _channelStates.TryGetValue(message.Payload.ChannelId, out var channelState);

        // In this case we can only handle messages that are opening a channel
        switch (message.Type)
        {
            case MessageTypes.OPEN_CHANNEL:
                // Handle opening channel
                var openChannelMessage = message as OpenChannel1Message
                                         ?? throw new ChannelException("Error boxing message to OpenChannel1Message");
                return HandleOpenChannel1Message(channelState, openChannelMessage, negotiatedFeatures);
            default:
                throw new ChannelException("Unknown message type");
        }
    }

    private IChannelMessage HandleOpenChannel1Message(ChannelState channelState, OpenChannel1Message message,
                                                      FeatureOptions negotiatedFeatures)
    {
        var payload = message.Payload;

        if (channelState != ChannelState.NONE)
            throw new ChannelException("There's already a channel with this id");

        // Check if there's a temporary channel for this peer
        var temporaryChannelTuple = (payload.FundingPubKey, payload.ChannelId);
        if (!_temporaryChannelStates.TryGetValue(temporaryChannelTuple, out channelState))
        {
            _temporaryChannelStates.Add(temporaryChannelTuple, ChannelState.V1_OPENING);
        }
        else if (channelState != ChannelState.V1_OPENING)
            throw new ChannelException("Channel is already being negotiated with peer");

        // Create the channel
        var channel = _channelFactory.CreateChannelV1AsNonInitiator(message, negotiatedFeatures);

        // Add channel to dictionaries
        _temporaryChannelLastReceivedMessage[temporaryChannelTuple] = message;
        _temporaryChannels[temporaryChannelTuple] = channel;

        return null!;//openChannelReplyMessage;
    }
}