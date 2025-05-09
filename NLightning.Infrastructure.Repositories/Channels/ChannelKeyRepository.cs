using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace NLightning.Infrastructure.Repositories.Channels;

using Domain.Channels.Repositories;
using Domain.ValueObjects;
using Persistence.Contexts;
using Persistence.Entities;

using NLightning.Domain.Channels;

public class ChannelKeyRepository: IChannelKeyRepository
{
    private readonly NLightningContext _context;

    public ChannelKeyRepository(NLightningContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<ChannelKeyData?> GetByKeyIndexForChannelAsync(ChannelId channelId, CancellationToken ct = default)
    {
        var channelKeyDataEntity = await _context
            .ChannelKeyData
            .FirstOrDefaultAsync(e => e.ChannelId == channelId, ct);

        return channelKeyDataEntity == null
            ? null
            : MapToDomainEntity(channelKeyDataEntity);
    }

    private static ChannelKeyData MapToDomainEntity(ChannelKeyDataEntity dataEntity)
    {
        return new ChannelKeyData(new ChannelId(dataEntity.ChannelId), dataEntity.KeyIndex, 
                                  new PubKey(dataEntity.FundingPubKey), new PubKey(dataEntity.RevocationBasepoint),
                                  new PubKey(dataEntity.PaymentBasepoint),
                                  new PubKey(dataEntity.DelayedPaymentBasepoint), new PubKey(dataEntity.HtlcBasepoint));
    }
    
    private static ChannelKeyDataEntity MapToDataEntity(ChannelKeyData domainEntity, Guid? existingId = null)
    {
        var dataEntity = existingId.HasValue
            ? new ChannelKeyDataEntity { Id = existingId.Value }
            : new ChannelKeyDataEntity { Id = Guid.NewGuid() };

        dataEntity.ChannelId = domainEntity.ChannelId;
        dataEntity.KeyIndex = domainEntity.KeyIndex;
        dataEntity.FundingPubKey = domainEntity.FundingPubKey.ToBytes();
        dataEntity.RevocationBasepoint = domainEntity.RevocationBasepoint.ToBytes();
        dataEntity.PaymentBasepoint = domainEntity.PaymentBasepoint.ToBytes();
        dataEntity.DelayedPaymentBasepoint = domainEntity.DelayedPaymentBasepoint.ToBytes();
        dataEntity.HtlcBasepoint = domainEntity.HtlcBasepoint.ToBytes();

        return dataEntity;
    }
}