using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using NLightning.Application.Channels.Handlers.Interfaces;
using NLightning.Domain.Channels.Enums;
using NLightning.Domain.Channels.Interfaces;
using NLightning.Domain.Channels.Models;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Enums;
using NLightning.Domain.Exceptions;
using NLightning.Domain.Node.Options;
using NLightning.Domain.Persistence.Interfaces;
using NLightning.Domain.Protocol.Messages;
using NLightning.Domain.Protocol.Messages.Interfaces;

namespace NLightning.Application.Channels.Handlers;

public class ChannelReadyMessageHandler : IChannelMessageHandler<ChannelReadyMessage>
{
    private readonly IChannelMemoryRepository _channelMemoryRepository;
    private readonly ILogger<ChannelReadyMessageHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ChannelReadyMessageHandler(IChannelMemoryRepository channelMemoryRepository,
                                      ILogger<ChannelReadyMessageHandler> logger, IUnitOfWork unitOfWork)
    {
        _channelMemoryRepository = channelMemoryRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IChannelMessage?> HandleAsync(ChannelReadyMessage message, ChannelState currentState,
                                                    FeatureOptions negotiatedFeatures, CompactPubKey peerPubKey)
    {
        _logger.LogTrace("Processing ChannelReadyMessage with ChannelId: {ChannelId} from Peer: {PeerPubKey}",
                         message.Payload.ChannelId, peerPubKey);

        var payload = message.Payload;

        if (currentState is not (ChannelState.V1FundingSigned
                              or ChannelState.ReadyForThem
                              or ChannelState.ReadyForUs
                              or ChannelState.Open))
            throw new ChannelErrorException("Channel had the wrong state", payload.ChannelId,
                                            "This channel is not ready to be opened");

        // Check if there's a channel for this peer
        if (!_channelMemoryRepository.TryGetChannel(payload.ChannelId, out var channel) || channel is null)
            throw new ChannelErrorException("Channel not found", payload.ChannelId,
                                            "This channel is not ready to be opened");

        var mustUseScidAlias = channel.ChannelConfig.UseScidAlias > FeatureSupport.No;
        if (mustUseScidAlias && message.ShortChannelIdTlv is null)
            throw new ChannelWarningException("No ShortChannelIdTlv provided",
                                              payload.ChannelId,
                                              "This channel requires a ShortChannelIdTlv to be provided");

        // Store their new per-commitment point
        if (channel.RemoteKeySet.CurrentPerCommitmentIndex == 0)
            channel.RemoteKeySet.UpdatePerCommitmentPoint(payload.SecondPerCommitmentPoint);

        // Handle ScidAlias
        if (currentState is ChannelState.Open or ChannelState.ReadyForThem)
        {
            if (mustUseScidAlias)
            {
                if (ShouldReplaceAlias())
                {
                    var oldAlias = channel.RemoteAlias;
                    channel.RemoteAlias = message.ShortChannelIdTlv!.ShortChannelId;

                    _logger.LogDebug("Updated remote alias for channel {ChannelId} from {OldAlias} to {NewAlias}",
                                     payload.ChannelId, oldAlias, channel.RemoteAlias);

                    await PersistChannelAsync(channel);
                }
                else
                {
                    _logger.LogDebug(
                        "Keeping existing remote alias {ExistingAlias} for channel {ChannelId}", channel.RemoteAlias,
                        payload.ChannelId);
                }
            }
            else
                _logger.LogDebug("Received duplicate ChannelReady message for channel {ChannelId} in Open state",
                                 payload.ChannelId);

            return null; // No further action needed, we are already open
        }

        if (channel.IsInitiator) // Handle state transitions based on whether we are the initiator
        {
            // We already sent our ChannelReady, now they sent theirs
            if (currentState == ChannelState.ReadyForUs)
            {
                // Valid transition: ReadyForUs -> Open
                channel.UpdateState(ChannelState.Open);
                await PersistChannelAsync(channel);

                _logger.LogInformation("Channel {ChannelId} is now open (we are initiator)", payload.ChannelId);

                // TODO: Notify application layer that channel is fully open
                // TODO: Update routing tables

                return null;
            }

            // Invalid state for initiator receiving ChannelReady
            _logger.LogError(
                "Received ChannelReady message for channel {ChannelId} in invalid state {CurrentState} (we are initiator). Expected: ReadyForUs",
                payload.ChannelId, currentState);

            throw new ChannelErrorException($"Unexpected ChannelReady message in state {currentState}",
                                            payload.ChannelId,
                                            "Protocol violation: unexpected ChannelReady message");
        }

        if (currentState == ChannelState.V1FundingSigned) // We are not the initiator
        {
            // First ChannelReady from initiator
            // Valid transition: V1FundingSigned -> ReadyForThem
            channel.UpdateState(ChannelState.ReadyForThem);
            await PersistChannelAsync(channel);

            _logger.LogInformation(
                "Received ChannelReady from initiator for channel {ChannelId}, waiting for funding confirmation",
                payload.ChannelId);

            return null;
        }

        // Invalid state for non-initiator receiving ChannelReady
        _logger.LogError(
            "Received ChannelReady message for channel {ChannelId} in invalid state {CurrentState} (we are not initiator). Expected: V1FundingSigned or ReadyForThem",
            payload.ChannelId, currentState);

        throw new ChannelErrorException($"Unexpected ChannelReady message in state {currentState}",
                                        payload.ChannelId,
                                        "Protocol violation: unexpected ChannelReady message");
    }

    /// <summary>
    /// Persists a channel to the database using a scoped Unit of Work
    /// </summary>
    private async Task PersistChannelAsync(ChannelModel channel)
    {
        try
        {
            // Check if the channel already exists
            _ = await _unitOfWork.ChannelDbRepository.GetByIdAsync(channel.ChannelId)
             ?? throw new ChannelWarningException("Channel not found in database", channel.ChannelId,
                                                  "Sorry, we had an internal error");
            await _unitOfWork.ChannelDbRepository.UpdateAsync(channel);
            await _unitOfWork.SaveChangesAsync();

            _channelMemoryRepository.UpdateChannel(channel);

            _logger.LogDebug("Successfully persisted channel {ChannelId} to database", channel.ChannelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist channel {ChannelId} to database", channel.ChannelId);
            throw;
        }
    }

    private static bool ShouldReplaceAlias()
    {
        return RandomNumberGenerator.GetInt32(0, 2) switch
        {
            0 => true,
            _ => false
        };
    }
}