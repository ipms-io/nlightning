using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NLightning.Application.Channels.Managers;

using Bitcoin.Interfaces;
using Domain.Bitcoin.Interfaces;
using Domain.Channels.Enums;
using Domain.Channels.Interfaces;
using Domain.Channels.Models;
using Domain.Channels.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Exceptions;
using Domain.Node.Options;
using Domain.Persistence.Interfaces;
using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Messages.Interfaces;
using Domain.Protocol.Tlv;
using Domain.Transactions.Enums;
using Domain.Transactions.Interfaces;

public class ChannelManager : IChannelManager
{
    private readonly Dictionary<ChannelId, ChannelModel> _channels = [];
    private readonly Dictionary<ChannelId, ChannelState> _channelStates = [];
    private readonly Dictionary<ChannelId, IChannelMessage> _channelLastReceivedMessage = [];
    private readonly Dictionary<(CompactPubKey, ChannelId), ChannelModel> _temporaryChannels = [];
    private readonly Dictionary<(CompactPubKey, ChannelId), ChannelState> _temporaryChannelStates = [];
    private readonly Dictionary<(CompactPubKey, ChannelId), IChannelMessage> _temporaryChannelLastReceivedMessage = [];

    private readonly IChannelFactory _channelFactory;
    private readonly IChannelIdFactory _channelIdFactory;
    private readonly ICommitmentTransactionBuilder _commitmentTransactionBuilder;
    private readonly ICommitmentTransactionModelFactory _commitmentTransactionModelFactory;
    private readonly ILightningSigner _lightningSigner;
    private readonly ILogger<ChannelManager> _logger;
    private readonly IMessageFactory _messageFactory;
    private readonly NodeOptions _nodeOptions;
    private readonly IServiceProvider _serviceProvider;

    public ChannelManager(IChannelFactory channelFactory, IChannelIdFactory channelIdFactory,
                          ICommitmentTransactionBuilder commitmentTransactionBuilder,
                          ICommitmentTransactionModelFactory commitmentTransactionModelFactory,
                          ILightningSigner lightningSigner, ILogger<ChannelManager> logger,
                          IMessageFactory messageFactory, NodeOptions nodeOptions, IServiceProvider serviceProvider)
    {
        _channelFactory = channelFactory;
        _channelIdFactory = channelIdFactory;
        _commitmentTransactionBuilder = commitmentTransactionBuilder;
        _commitmentTransactionModelFactory = commitmentTransactionModelFactory;
        _messageFactory = messageFactory;
        _nodeOptions = nodeOptions;
        _serviceProvider = serviceProvider;
        _lightningSigner = lightningSigner;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        // Load existing channels from the database
        using var scope = _serviceProvider.CreateScope();
        using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            var existingChannels = await unitOfWork.ChannelDbRepository.GetReadyChannelsAsync();

            var channelModels = existingChannels as ChannelModel[] ?? existingChannels.ToArray();
            foreach (var channel in channelModels)
            {
                if (channel.FundingOutput.TransactionId is null)
                {
                    _logger.LogError("Channel {ChannelId} has no funding transaction id, skipping", channel.ChannelId);
                    continue;
                }

                _channels[channel.ChannelId] = channel;
                _channelStates[channel.ChannelId] = channel.State;

                // Register channel with signer
                var channelSigningInfo = new ChannelSigningInfo(
                    channel.FundingOutput.TransactionId!.Value,
                    channel.FundingOutput.Index!.Value,
                    channel.FundingOutput.Amount,
                    channel.LocalKeySet.FundingCompactPubKey,
                    channel.RemoteKeySet.FundingCompactPubKey,
                    channel.LocalKeySet.KeyIndex
                );
                _lightningSigner.RegisterChannel(channel.ChannelId, channelSigningInfo);
            }

            _logger.LogInformation("Loaded {ChannelCount} channels from database", channelModels.Length);
        }
        catch (Exception e)
        {
            throw new CriticalException("Failed to initialize channels from database", e);
        }
    }

    public async Task<IChannelMessage> HandleChannelMessageAsync(IChannelMessage message,
                                                                 FeatureOptions negotiatedFeatures,
                                                                 CompactPubKey peerPubKey)
    {
        // Check if the channel exists on the state dictionary
        _channelStates.TryGetValue(message.Payload.ChannelId, out var channelState);

        // In this case we can only handle messages that are opening a channel
        switch (message.Type)
        {
            case MessageTypes.OpenChannel:
                // Handle opening channel message
                var openChannelMessage =
                    message as OpenChannel1Message
                 ?? throw new ChannelErrorException("Error boxing message to OpenChannel1Message",
                                                    "Sorry, we had an internal error");
                return await HandleOpenChannel1MessageAsync(channelState, openChannelMessage, negotiatedFeatures,
                                                            peerPubKey);
            case MessageTypes.FundingCreated:
                // Handle the funding-created message
                var fundingCreatedMessage =
                    message as FundingCreatedMessage
                 ?? throw new ChannelErrorException("Error boxing message to FundingCreatedMessage",
                                                    "Sorry, we had an internal error");
                return await HandleFundingCreatedMessageAsync(channelState, fundingCreatedMessage, peerPubKey);
            default:
                throw new ChannelErrorException("Unknown message type", "Sorry, we had an internal error");
        }
    }

    private async Task<AcceptChannel1Message> HandleOpenChannel1MessageAsync(ChannelState channelState,
                                                                             OpenChannel1Message message,
                                                                             FeatureOptions negotiatedFeatures,
                                                                             CompactPubKey peerPubKey)
    {
        _logger.LogTrace("Processing OpenChannel1Message with ChannelId: {ChannelId} from Peer: {PeerPubKey}",
                         message.Payload.ChannelId, peerPubKey);

        var payload = message.Payload;

        if (channelState != ChannelState.None)
            throw new ChannelErrorException("A channel with this id already exists", payload.ChannelId);

        // Check if there's a temporary channel for this peer
        var temporaryChannelTuple = (peerPubKey, payload.ChannelId);
        if (!_temporaryChannelStates.TryGetValue(temporaryChannelTuple, out channelState))
        {
            _temporaryChannelStates.Add(temporaryChannelTuple, ChannelState.V1Opening);
        }
        else if (channelState != ChannelState.V1Opening)
        {
            throw new ChannelErrorException("Channel had the wrong state", payload.ChannelId,
                                            "This channel is already being negotiated with peer");
        }

        // Create the channel
        var channel = await _channelFactory.CreateChannelV1AsNonInitiatorAsync(message, negotiatedFeatures, peerPubKey);

        _logger.LogTrace("Created Channel with fundingPubKey: {fundingPubKey}",
                         channel.LocalKeySet.FundingCompactPubKey);

        // Add the channel to dictionaries
        _temporaryChannelLastReceivedMessage[temporaryChannelTuple] = message;
        _temporaryChannels[temporaryChannelTuple] = channel;

        // Create UpfrontShutdownScriptTlv if needed
        UpfrontShutdownScriptTlv? upfrontShutdownScriptTlv = null;
        if (channel.LocalUpfrontShutdownScript is not null)
            upfrontShutdownScriptTlv = new UpfrontShutdownScriptTlv(channel.LocalUpfrontShutdownScript.Value);

        // TODO: Create the ChannelTypeTlv

        // Create the reply message
        var acceptChannel1ReplyMessage = _messageFactory
           .CreateAcceptChannel1Message(channel.ChannelConfig.ChannelReserveAmount!, null,
                                        channel.LocalKeySet.DelayedPaymentCompactBasepoint,
                                        channel.LocalKeySet.CurrentPerCommitmentCompactPoint,
                                        channel.LocalKeySet.FundingCompactPubKey,
                                        channel.LocalKeySet.HtlcCompactBasepoint,
                                        channel.ChannelConfig.MaxAcceptedHtlcs,
                                        channel.ChannelConfig.MaxHtlcAmountInFlight, channel.ChannelConfig.MinimumDepth,
                                        channel.LocalKeySet.PaymentCompactBasepoint,
                                        channel.LocalKeySet.RevocationCompactBasepoint, channel.ChannelId,
                                        channel.ChannelConfig.ToSelfDelay, upfrontShutdownScriptTlv);

        return acceptChannel1ReplyMessage;
    }

    private async Task<FundingSignedMessage> HandleFundingCreatedMessageAsync(ChannelState channelState,
                                                                              FundingCreatedMessage message,
                                                                              CompactPubKey peerPubKey)
    {
        _logger.LogTrace("Processing FundingCreatedMessage with ChannelId: {ChannelId} from Peer: {PeerPubKey}",
                         message.Payload.ChannelId, peerPubKey);

        var payload = message.Payload;

        if (channelState != ChannelState.None)
            throw new ChannelErrorException("A channel with this id already exists", payload.ChannelId);

        // Check if there's a temporary channel for this peer
        var temporaryChannelTuple = (peerPubKey, payload.ChannelId);
        if (!_temporaryChannelStates.TryGetValue(temporaryChannelTuple, out channelState))
            throw new ChannelErrorException("This channel has never been negotiated", payload.ChannelId);

        if (channelState != ChannelState.V1Opening)
            throw new ChannelErrorException("Channel had the wrong state", payload.ChannelId,
                                            "This channel is already being negotiated with peer");

        // Get the channel and set missing props
        var channel = _temporaryChannels[temporaryChannelTuple];
        channel.FundingOutput.TransactionId = payload.FundingTxId;
        channel.FundingOutput.Index = payload.FundingOutputIndex;

        // Create a new channelId
        var channelId = _channelIdFactory.CreateV1(payload.FundingTxId, payload.FundingOutputIndex);

        // Register the channel with the signer
        var channelSigningInfo = new ChannelSigningInfo(
            payload.FundingTxId,
            payload.FundingOutputIndex,
            channel.FundingOutput.Amount,
            channel.LocalKeySet.FundingCompactPubKey,
            channel.RemoteKeySet.FundingCompactPubKey,
            channel.LocalKeySet.KeyIndex
        );
        _lightningSigner.RegisterChannel(channelId, channelSigningInfo);

        // Generate the base commitment transactions
        var localCommitmentTransaction =
            _commitmentTransactionModelFactory.CreateCommitmentTransactionModel(channel, CommitmentSide.Local);
        var remoteCommitmentTransaction =
            _commitmentTransactionModelFactory.CreateCommitmentTransactionModel(channel, CommitmentSide.Remote);

        // Build the output and the transactions
        var localUnsignedCommitmentTransaction = _commitmentTransactionBuilder.Build(localCommitmentTransaction);
        var remoteUnsignedCommitmentTransaction = _commitmentTransactionBuilder.Build(remoteCommitmentTransaction);

        // Validate remote signature for our local commitment transaction
        _lightningSigner.ValidateSignature(channelId, payload.Signature, localUnsignedCommitmentTransaction);

        // Sign our remote commitment transaction
        var ourSignature = _lightningSigner.SignTransaction(channelId, remoteUnsignedCommitmentTransaction);

        channel.UpdateState(ChannelState.V1FundingSigned);
        // Save to the database
        await PersistChannelAsync(channelId, channel);

        // Create the funding signed message
        var fundingSignedMessage =
            _messageFactory.CreatedFundingSignedMessage(channelId, ourSignature);

        // Add the channel to the dictionary
        _channelStates.Add(channelId, ChannelState.V1FundingSigned);
        _channelLastReceivedMessage.Add(channelId, fundingSignedMessage);
        _channels.Add(channelId, channel);

        // Remove the temporary channel
        _temporaryChannelStates.Remove(temporaryChannelTuple);
        _temporaryChannelLastReceivedMessage.Remove(temporaryChannelTuple);
        _temporaryChannels.Remove(temporaryChannelTuple);

        return fundingSignedMessage;
    }

    /// <summary>
    /// Persists a channel to the database using a scoped Unit of Work
    /// </summary>
    private async Task PersistChannelAsync(ChannelId channelId, ChannelModel channel)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            // Check if the channel already exists
            var existingChannel = await unitOfWork.ChannelDbRepository.GetByIdAsync(channelId);
            if (existingChannel == null)
            {
                await unitOfWork.ChannelDbRepository.AddAsync(channel);
            }
            else
            {
                await unitOfWork.ChannelDbRepository.UpdateAsync(channel);
            }

            await unitOfWork.SaveChangesAsync();

            _logger.LogDebug("Successfully persisted channel {ChannelId} to database", channelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist channel {ChannelId} to database", channelId);
            throw;
        }
    }
}