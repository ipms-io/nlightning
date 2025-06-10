using Microsoft.EntityFrameworkCore;

namespace NLightning.Infrastructure.Repositories.Database.Bitcoin;

using Domain.Bitcoin.Interfaces;
using Domain.Bitcoin.ValueObjects;
using Persistence.Contexts;
using Persistence.Entities.Bitcoin;

public class BlockchainStateDbRepository(NLightningDbContext context)
    : BaseDbRepository<BlockchainStateEntity>(context), IBlockchainStateDbRepository
{
    public async Task AddOrUpdateAsync(BlockchainState blockchainState)
    {
        var existingEntity = await DbSet.AsNoTracking().FirstOrDefaultAsync();
        if (existingEntity is null)
        {
            var entity = MapDomainToEntity(blockchainState, Guid.NewGuid());
            Insert(entity);
        }
        else
        {
            var entity = MapDomainToEntity(blockchainState, existingEntity.Id);
            Update(entity);
        }
    }

    public async Task<BlockchainState?> GetStateAsync()
    {
        var entity = await DbSet.AsNoTracking().FirstOrDefaultAsync();
        return entity is null ? null : MapEntityToDomain(entity);
    }

    private static BlockchainStateEntity MapDomainToEntity(BlockchainState blockchainState, Guid id)
    {
        return new BlockchainStateEntity
        {
            Id = id,
            LastProcessedHeight = blockchainState.LastProcessedHeight,
            LastProcessedBlockHash = blockchainState.LastProcessedBlockHash,
            LastProcessedAt = blockchainState.LastProcessedAt
        };
    }

    private static BlockchainState MapEntityToDomain(BlockchainStateEntity entity)
    {
        return new BlockchainState(entity.LastProcessedHeight, entity.LastProcessedBlockHash, entity.LastProcessedAt);
    }
}