namespace NLightning.Infrastructure.Repositories.Database.Channels;

using Domain.Channels.Interfaces;
using Domain.Channels.Models;
using Domain.Channels.ValueObjects;
using Persistence.Contexts;
using Persistence.Entities;

public class ChannelKeySetDbRepository : BaseDbRepository<ChannelKeySetEntity>, IChannelKeySetDbRepository
{
    public ChannelKeySetDbRepository(NLightningDbContext context) : base(context)
    {
    }

    public void Add(ChannelId channelId, bool isLocal, ChannelKeySetModel keySet)
    {
        var keySetEntity = MapDomainToEntity(channelId, isLocal, keySet);
        Insert(keySetEntity);
    }

    public void Update(ChannelId channelId, bool isLocal, ChannelKeySetModel keySet)
    {
        var keySetEntity = MapDomainToEntity(channelId, isLocal, keySet);
        base.Update(keySetEntity);
    }

    public Task DeleteAsync(ChannelId channelId, bool isLocal)
    {
        return DeleteByIdAsync((channelId, isLocal));
    }

    public async Task<ChannelKeySetModel?> GetByIdAsync(ChannelId channelId, bool isLocal)
    {
        var keySetEntity = await base.GetByIdAsync((channelId, isLocal));

        if (keySetEntity is null)
            return null;

        return MapEntityToDomain(keySetEntity);
    }

    internal static ChannelKeySetEntity MapDomainToEntity(ChannelId channelId, bool isLocal, ChannelKeySetModel keySet)
    {
        return new ChannelKeySetEntity
        {
            // Base information
            ChannelId = channelId,
            IsLocal = isLocal,
            KeyIndex = isLocal ? keySet.KeyIndex : 0,

            // PubKeys and Basepoints
            FundingPubKey = keySet.FundingCompactPubKey,
            RevocationBasepoint = keySet.RevocationCompactBasepoint,
            PaymentBasepoint = keySet.PaymentCompactBasepoint,
            DelayedPaymentBasepoint = keySet.DelayedPaymentCompactBasepoint,
            HtlcBasepoint = keySet.HtlcCompactBasepoint,

            // Current commitment state
            CurrentPerCommitmentPoint = keySet.CurrentPerCommitmentCompactPoint,
            CurrentPerCommitmentIndex = keySet.CurrentPerCommitmentIndex,
            LastRevealedPerCommitmentSecret = keySet.LastRevealedPerCommitmentSecret
        };
    }

    internal static ChannelKeySetModel MapEntityToDomain(ChannelKeySetEntity entity)
    {
        return new ChannelKeySetModel(entity.KeyIndex, entity.FundingPubKey, entity.RevocationBasepoint,
                                      entity.PaymentBasepoint,
                                      entity.DelayedPaymentBasepoint, entity.HtlcBasepoint, null,
                                      entity.CurrentPerCommitmentPoint, entity.CurrentPerCommitmentIndex,
                                      entity.LastRevealedPerCommitmentSecret);
    }
}