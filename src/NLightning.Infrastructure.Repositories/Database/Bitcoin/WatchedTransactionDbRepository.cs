using Microsoft.EntityFrameworkCore;

namespace NLightning.Infrastructure.Repositories.Database.Bitcoin;

using Domain.Bitcoin.Interfaces;
using Domain.Bitcoin.Transactions.Models;
using Persistence.Contexts;
using Persistence.Entities.Bitcoin;

public class WatchedTransactionDbRepository(NLightningDbContext context)
    : BaseDbRepository<WatchedTransactionEntity>(context), IWatchedTransactionDbRepository
{
    public void Add(WatchedTransactionModel watchedTransactionModel)
    {
        var watchedTransactionEntity = MapDomainToEntity(watchedTransactionModel);

        if (watchedTransactionEntity.CreatedAt.Equals(DateTime.MinValue))
            watchedTransactionEntity.CreatedAt = DateTime.UtcNow;

        Insert(watchedTransactionEntity);
    }

    public void Update(WatchedTransactionModel watchedTransactionModel)
    {
        var watchedTransactionEntity = MapDomainToEntity(watchedTransactionModel);
        Update(watchedTransactionEntity);
    }

    public async Task<IEnumerable<WatchedTransactionModel>> GetAllPendingAsync()
    {
        var entities = await DbSet
                            .AsNoTracking()
                            .Where(x => x.CompletedAt == null)
                            .ToListAsync();

        return entities.Select(entity => MapEntityToDomain(entity));
    }

    private static WatchedTransactionEntity MapDomainToEntity(WatchedTransactionModel watchedTransactionModel)
    {
        return new WatchedTransactionEntity
        {
            ChannelId = watchedTransactionModel.ChannelId,
            TransactionId = watchedTransactionModel.TransactionId,
            RequiredDepth = watchedTransactionModel.RequiredDepth,
            FirstSeenAtHeight = watchedTransactionModel.FirstSeenAtHeight,
            TransactionIndex = watchedTransactionModel.TransactionIndex,
            CompletedAt = watchedTransactionModel.IsCompleted ? DateTime.UtcNow : null
        };
    }

    private static WatchedTransactionModel MapEntityToDomain(WatchedTransactionEntity entity)
    {
        var model = new WatchedTransactionModel(entity.ChannelId, entity.TransactionId, entity.RequiredDepth);

        if (entity.CompletedAt.HasValue)
            model.MarkAsCompleted();

        if (entity.FirstSeenAtHeight.HasValue && entity.TransactionIndex.HasValue)
            model.SetHeightAndIndex(entity.FirstSeenAtHeight.Value, entity.TransactionIndex.Value);

        return model;
    }
}