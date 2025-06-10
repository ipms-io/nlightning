using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Infrastructure.Bitcoin.Wallet.Interfaces;

namespace NLightning.Application.Channels.Managers;

using Domain.Bitcoin.Interfaces;
using Domain.Channels.Constants;
using Domain.Channels.Enums;
using Domain.Channels.Interfaces;
using Domain.Channels.Models;
using Domain.Channels.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Exceptions;
using Domain.Node.Options;
using Domain.Persistence.Interfaces;
using Domain.Protocol.Constants;
using Domain.Protocol.Messages;
using Handlers.Interfaces;

public class ChannelManager : IChannelManager
{
    private readonly IChannelMemoryRepository _channelMemoryRepository;
    private readonly ILogger<ChannelManager> _logger;
    private readonly ILightningSigner _lightningSigner;
    private readonly IServiceProvider _serviceProvider;

    public ChannelManager(IBlockchainMonitor blockchainMonitor, IChannelMemoryRepository channelMemoryRepository,
                          ILogger<ChannelManager> logger, ILightningSigner lightningSigner,
                          IServiceProvider serviceProvider)
    {
        _channelMemoryRepository = channelMemoryRepository;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _lightningSigner = lightningSigner;

        blockchainMonitor.OnNewBlockDetected += (_, args) =>
            ForgetOldChannelByBlockHeightAsync(args.Height)
               .ContinueWith((task) =>
                {
                    _logger.LogDebug(task.Exception, "Error while forgetting stale channels.");
                }, TaskContinuationOptions.OnlyOnFaulted);

        blockchainMonitor.OnTransactionConfirmed += (_, args) =>
        {
            // Check if the transaction is a funding transaction for any channel
            if (_channelMemoryRepository.TryGetChannel(args.WatchedTransaction.ChannelId, out var channel)
             && channel is not null)
            {
                // If the channel is not confirmed, update its state
                channel.UpdateState(ChannelState.ReadyForUs);
                _channelMemoryRepository.UpdateChannel(channel);
                _logger.LogInformation("Channel {ChannelId} funding transaction confirmed", channel.ChannelId);
            }
        };
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

                _channelMemoryRepository.AddChannel(channel);

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

    public async Task<IChannelMessage?> HandleChannelMessageAsync(IChannelMessage message,
                                                                  FeatureOptions negotiatedFeatures,
                                                                  CompactPubKey peerPubKey)
    {
        using var scope = _serviceProvider.CreateScope();

        // Check if the channel exists on the state dictionary
        _channelMemoryRepository.TryGetChannelState(message.Payload.ChannelId, out var currentState);

        // In this case we can only handle messages that are opening a channel
        switch (message.Type)
        {
            case MessageTypes.OpenChannel:
                // Handle opening channel message
                var openChannel1Message = message as OpenChannel1Message
                                       ?? throw new ChannelErrorException("Error boxing message to OpenChannel1Message",
                                                                          "Sorry, we had an internal error");
                return await GetChannelMessageHandler<OpenChannel1Message>(scope)
                          .HandleAsync(openChannel1Message, currentState, negotiatedFeatures, peerPubKey);

            case MessageTypes.FundingCreated:
                // Handle the funding-created message
                var fundingCreatedMessage = message as FundingCreatedMessage
                                         ?? throw new ChannelErrorException(
                                                "Error boxing message to FundingCreatedMessage",
                                                "Sorry, we had an internal error");
                return await GetChannelMessageHandler<FundingCreatedMessage>(scope)
                          .HandleAsync(fundingCreatedMessage, currentState, negotiatedFeatures, peerPubKey);

            case MessageTypes.ChannelReady:
                // Handle channel ready message
                var channelReadyMessage = message as ChannelReadyMessage
                                       ?? throw new ChannelErrorException("Error boxing message to ChannelReadyMessage",
                                                                          "Sorry, we had an internal error");
                return await GetChannelMessageHandler<ChannelReadyMessage>(scope)
                          .HandleAsync(channelReadyMessage, currentState, negotiatedFeatures, peerPubKey);
            default:
                throw new ChannelErrorException("Unknown message type", "Sorry, we had an internal error");
        }
    }

    public async Task ForgetOldChannelByBlockHeightAsync(uint blockHeight)
    {
        var heightLimit = (int)blockHeight - ChannelConstants.MaxUnconfirmedChannelAge;
        if (heightLimit < 0)
        {
            _logger.LogDebug("Block height {BlockHeight} is too low to forget channels", blockHeight);
            return;
        }

        var staleChannels = _channelMemoryRepository.FindChannels(c => c.FundingCreatedAtBlockHeight <= heightLimit);

        _logger.LogDebug(
            "Forgetting stale channels created before block height {HeightLimit}, found {StaleChannelCount} channels",
            heightLimit, staleChannels.Count);

        foreach (var staleChannel in staleChannels)
        {
            _logger.LogInformation(
                "Forgetting stale channel {ChannelId} with funding created at block height {BlockHeight}",
                staleChannel.ChannelId, staleChannel.FundingCreatedAtBlockHeight);

            // Set states
            staleChannel.UpdateState(ChannelState.Stale);
            _channelMemoryRepository.UpdateChannel(staleChannel);

            // Persist on Db
            try
            {
                await PersistChannelAsync(staleChannel);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to persist stale channel {ChannelId} to database at height {currentHeight}",
                                 staleChannel.ChannelId, blockHeight);
            }
        }
    }

    private IChannelMessageHandler<T> GetChannelMessageHandler<T>(IServiceScope scope)
        where T : IChannelMessage
    {
        var handler = scope.ServiceProvider.GetRequiredService<IChannelMessageHandler<T>>() ??
                      throw new ChannelErrorException($"No handler found for message type {typeof(T).FullName}",
                                                      "Sorry, we had an internal error");
        return handler;
    }

    /// <summary>
    /// Persists a channel to the database using a scoped Unit of Work
    /// </summary>
    private async Task PersistChannelAsync(ChannelModel channel)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            // Check if the channel already exists

            _ = await unitOfWork.ChannelDbRepository.GetByIdAsync(channel.ChannelId)
             ?? throw new ChannelWarningException("Channel not found", channel.ChannelId);
            await unitOfWork.ChannelDbRepository.UpdateAsync(channel);
            await unitOfWork.SaveChangesAsync();

            // Remove from dictionaries
            _channelMemoryRepository.RemoveChannel(channel.ChannelId);

            _logger.LogDebug("Successfully persisted channel {ChannelId} to database", channel.ChannelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist channel {ChannelId} to database", channel.ChannelId);
            throw;
        }
    }
}