using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.Interfaces;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Money;
using NLightning.Infrastructure.Persistence.Contexts;
using NLightning.Infrastructure.Persistence.Entities;

namespace NLightning.Infrastructure.Repositories.Database.Channels;

public class ChannelConfigDbRepository(NLightningDbContext context)
    : BaseDbRepository<ChannelConfigEntity>(context), IChannelConfigDbRepository
{
    public void Add(ChannelId channelId, ChannelConfig config)
    {
        var configEntity = MapDomainToEntity(channelId, config);
        Insert(configEntity);
    }

    public void Update(ChannelId channelId, ChannelConfig config)
    {
        var configEntity = MapDomainToEntity(channelId, config);
        base.Update(configEntity);
    }

    public Task DeleteAsync(ChannelId channelId)
    {
        return DeleteByIdAsync(channelId);
    }

    public async Task<ChannelConfig?> GetByChannelIdAsync(ChannelId channelId)
    {
        var configEntity = await GetByIdAsync(channelId);

        return configEntity == null ? null : MapEntityToDomain(configEntity);
    }

    internal static ChannelConfigEntity MapDomainToEntity(ChannelId channelId, ChannelConfig config)
    {
        return new ChannelConfigEntity
        {
            ChannelId = channelId,
            MinimumDepth = config.MinimumDepth,
            ToSelfDelay = config.ToSelfDelay,
            MaxAcceptedHtlcs = config.MaxAcceptedHtlcs,
            LocalDustLimitAmountSats = config.LocalDustLimitAmount.Satoshi,
            RemoteDustLimitAmountSats = config.RemoteDustLimitAmount.Satoshi,
            HtlcMinimumMsat = config.HtlcMinimumAmount.MilliSatoshi,
            ChannelReserveAmountSats = config.ChannelReserveAmount?.Satoshi,
            MaxHtlcAmountInFlight = config.MaxHtlcAmountInFlight.MilliSatoshi,
            FeeRatePerKwSatoshis = config.FeeRateAmountPerKw.Satoshi,
            OptionAnchorOutputs = config.OptionAnchorOutputs,
            LocalUpfrontShutdownScript = config.LocalUpfrontShutdownScript,
            RemoteUpfrontShutdownScript = config.RemoteShutdownScriptPubKey
        };
    }

    internal static ChannelConfig MapEntityToDomain(ChannelConfigEntity entity)
    {
        LightningMoney? channelReserveAmount = null;
        if (entity.ChannelReserveAmountSats.HasValue)
            channelReserveAmount = LightningMoney.Satoshis(entity.ChannelReserveAmountSats.Value);

        BitcoinScript? localUpfrontShutdownScript = null;
        if (entity.LocalUpfrontShutdownScript is not null)
            localUpfrontShutdownScript = entity.LocalUpfrontShutdownScript;

        BitcoinScript? remoteUpfrontShutdownScript = null;
        if (entity.RemoteUpfrontShutdownScript is not null)
            remoteUpfrontShutdownScript = entity.RemoteUpfrontShutdownScript;

        return new ChannelConfig(
            minimumDepth: entity.MinimumDepth,
            toSelfDelay: entity.ToSelfDelay,
            maxAcceptedHtlcs: entity.MaxAcceptedHtlcs,
            localDustLimitAmount: LightningMoney.Satoshis(entity.LocalDustLimitAmountSats),
            remoteDustLimitAmount: LightningMoney.Satoshis(entity.RemoteDustLimitAmountSats),
            htlcMinimumAmount: LightningMoney.MilliSatoshis(entity.HtlcMinimumMsat),
            channelReserveAmount: channelReserveAmount,
            maxHtlcAmountInFlight: LightningMoney.MilliSatoshis(entity.MaxHtlcAmountInFlight),
            feeRateAmountPerKw: LightningMoney.Satoshis(entity.FeeRatePerKwSatoshis),
            optionAnchorOutputs: entity.OptionAnchorOutputs,
            localUpfrontShutdownScript: localUpfrontShutdownScript,
            remoteShutdownScriptPubKey: remoteUpfrontShutdownScript
        );
    }
}