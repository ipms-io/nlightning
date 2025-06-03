using NLightning.Domain.Bitcoin.Interfaces;
using NLightning.Domain.Channels.Enums;
using NLightning.Domain.Channels.Interfaces;
using NLightning.Domain.Channels.Models;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Enums;
using NLightning.Domain.Exceptions;
using NLightning.Domain.Node;
using NLightning.Domain.Node.Options;
using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Protocol.Messages;
using NLightning.Domain.Protocol.Messages.Interfaces;
using NLightning.Domain.Protocol.Tlv;

namespace NLightning.Application.Channels.Managers;

public class ChannelManager : IChannelManager
{
    private readonly Dictionary<ChannelId, Channel> _channels = [];
    private readonly Dictionary<ChannelId, ChannelState> _channelStates = [];
    private readonly Dictionary<ChannelId, IChannelMessage> _channelLastReceivedMessage = [];
    private readonly Dictionary<(CompactPubKey, ChannelId), Channel> _temporaryChannels = [];
    private readonly Dictionary<(CompactPubKey, ChannelId), ChannelState> _temporaryChannelStates = [];
    private readonly Dictionary<(CompactPubKey, ChannelId), IChannelMessage> _temporaryChannelLastReceivedMessage = [];

    private readonly IChannelFactory _channelFactory;
    private readonly IChannelIdFactory _channelIdFactory;
    private readonly IMessageFactory _messageFactory;
    private readonly NodeOptions _nodeOptions;
    private readonly ILightningSigner _lightningSigner;

    public ChannelManager(IChannelFactory channelFactory, IChannelIdFactory channelIdFactory,
                          IMessageFactory messageFactory, NodeOptions nodeOptions,
                          ILightningSigner lightningSigner)
    {
        _channelFactory = channelFactory;
        _channelIdFactory = channelIdFactory;
        _messageFactory = messageFactory;
        _nodeOptions = nodeOptions;
        _lightningSigner = lightningSigner;
    }

    public async Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public IChannelMessage HandleChannelMessage(IChannelMessage message, FeatureOptions negotiatedFeatures,
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
                return HandleOpenChannel1Message(channelState, openChannelMessage, negotiatedFeatures, peerPubKey);
            // case MessageTypes.FundingCreated:
            //     // Handle the funding-created message
            //     var fundingCreatedMessage =
            //         message as FundingCreatedMessage
            //         ?? throw new ChannelErrorException("Error boxing message to FundingCreatedMessage",
            //                                            "Sorry, we had an internal error");
            //     return HandleFundingCreatedMessage(channelState, fundingCreatedMessage, peerPubKey);
            default:
                throw new ChannelErrorException("Unknown message type", "Sorry, we had an internal error");
        }
    }

    private AcceptChannel1Message HandleOpenChannel1Message(ChannelState channelState, OpenChannel1Message message,
                                                            FeatureOptions negotiatedFeatures, CompactPubKey peerPubKey)
    {
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
            throw new ChannelErrorException("Channel had the wrong state", payload.ChannelId,
                                            "This channel is already being negotiated with peer");

        // Create the channel
        var channel = _channelFactory.CreateChannelV1AsNonInitiator(message, negotiatedFeatures, peerPubKey);

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
           .CreateAcceptChannel1Message(channel.ChannelId, channel.ChannelConfig.ChannelReserveAmount!,
                                        channel.ChannelConfig.MinimumDepth, channel.ChannelConfig.MaxAcceptedHtlcs,
                                        channel.LocalKeySet.FundingCompactPubKey,
                                        channel.LocalKeySet.RevocationCompactBasepoint,
                                        channel.LocalKeySet.PaymentCompactBasepoint,
                                        channel.LocalKeySet.DelayedPaymentCompactBasepoint,
                                        channel.LocalKeySet.HtlcCompactBasepoint,
                                        channel.LocalKeySet.CurrentPerCommitmentCompactPoint,
                                        channel.ChannelConfig.MaxHtlcAmountInFlight,
                                        upfrontShutdownScriptTlv, null);

        return acceptChannel1ReplyMessage;
    }

    // private FundingSignedMessage HandleFundingCreatedMessage(ChannelState channelState, FundingCreatedMessage message,
    //                                                          CompactPubKey peerPubKey)
    // {
    //     var payload = message.Payload;
    //
    //     if (channelState != ChannelState.None)
    //         throw new ChannelErrorException("A channel with this id already exists", payload.ChannelId);
    //
    //     // Check if there's a temporary channel for this peer
    //     var temporaryChannelTuple = (peerPubKey, payload.ChannelId);
    //     if (!_temporaryChannelStates.TryGetValue(temporaryChannelTuple, out channelState))
    //         throw new ChannelErrorException("This channel has never been negotiated", payload.ChannelId);
    //
    //     if (channelState != ChannelState.V1Opening)
    //         throw new ChannelErrorException("Channel had the wrong state", payload.ChannelId,
    //                                         "This channel is already being negotiated with peer");
    //
    //     if (!payload.Signature.IsLowS)
    //         throw new ChannelErrorException("Signature is not low S", payload.ChannelId);
    //
    //     // Get the channel
    //     var channel = _temporaryChannels[temporaryChannelTuple];
    //
    //     if (channel.CommitmentTransaction is null)
    //         throw new ChannelErrorException("Commitment transaction is not set for channel", payload.ChannelId,
    //                                         "Sorry, we had an internal error");
    //
    //     // Create the new funding output
    //     var fundingOutput = _fundingOutputFactory
    //         .Create(channel.FundingOutput.Amount, payload.FundingOutputIndex, channel.LocalFundingPubKey,
    //                 channel.PeerFundingPubKey, payload.FundingTxId);
    //
    //     // Replace commitment transaction with new funding transaction
    //     try
    //     {
    //         channel.CommitmentTransaction.ReplaceFundingOutput(channel.FundingOutput, fundingOutput);
    //     }
    //     catch (Exception e)
    //     {
    //         throw new ChannelErrorException("Error replacing funding output", payload.ChannelId, e,
    //                                         "Sorry, we had an internal error");
    //     }
    //
    //     List<ECDSASignature>? signatures;
    //     try
    //     {
    //         signatures = channel.CommitmentTransaction
    //             .AppendRemoteSignatureAndSign(_lightningSigner, payload.Signature, channel.PeerFundingPubKey);
    //     }
    //     catch (Exception e)
    //     {
    //         throw new ChannelErrorException("Error appending remote signature", payload.ChannelId, e,
    //                                         "Sorry, we had an internal error");
    //     }
    //
    //     if (!channel.CommitmentTransaction.IsValid || signatures is not { Count: 2 })
    //         throw new ChannelErrorException("Commitment transaction is invalid", payload.ChannelId,
    //                                         "Sorry, we had an internal error");
    //
    //     // Get our signature
    //     var ourSignature = signatures
    //         .FirstOrDefault(s => !s.ToDER().SequenceEqual(payload.Signature.ToDER()))
    //         ?? throw new ChannelErrorException("Could not find our signature", channel.ChannelId,
    //                                            "Sorry, we had an internal error");
    //
    //     // Create a new channelId
    //     var channelId = _channelIdFactory.CreateV1(payload.FundingTxId.ToBytes(), payload.FundingOutputIndex);
    //
    //     // Create the funding signed message
    //     var fundingSignedMessage = _messageFactory.CreatedFundingSignedMessage(channelId, ourSignature);
    //
    //     // Add the channel to the dictionary
    //     _channelStates.Add(channelId, ChannelState.V1FundingSigned);
    //     _channelLastReceivedMessage.Add(channelId, fundingSignedMessage);
    //     _channels.Add(channelId, channel);
    //
    //     // Remove the temporary channel
    //     _temporaryChannelStates.Remove(temporaryChannelTuple);
    //     _temporaryChannelLastReceivedMessage.Remove(temporaryChannelTuple);
    //     _temporaryChannels.Remove(temporaryChannelTuple);
    //
    //     return fundingSignedMessage;
    // }
}