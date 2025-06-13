using Microsoft.EntityFrameworkCore;

namespace NLightning.Infrastructure.Repositories.Database.Bitcoin;

using Domain.Bitcoin.Interfaces;
using Domain.Bitcoin.ValueObjects;
using Persistence.Contexts;
using Persistence.Entities.Bitcoin;

public class BlockchainStateDbRepository(NLightningDbContext context)
    : BaseDbRepository<BlockchainStateEntity>(context), IBlockchainStateDbRepository
{
    public void Add(BlockchainState blockchainState)
    {
        var entity = MapDomainToEntity(blockchainState);
        Insert(entity);
    }

    public void Update(BlockchainState blockchainState)
    {
        var entity = MapDomainToEntity(blockchainState);
        Update(entity);
    }

    public async Task<BlockchainState?> GetStateAsync()
    {
        var entity = await DbSet.AsNoTracking().FirstOrDefaultAsync();
        return entity is null ? null : MapEntityToDomain(entity);
    }

    private static BlockchainStateEntity MapDomainToEntity(BlockchainState blockchainState)
    {
        return new BlockchainStateEntity
        {
            Id = blockchainState.Id,
            LastProcessedHeight = blockchainState.LastProcessedHeight,
            LastProcessedBlockHash = blockchainState.LastProcessedBlockHash,
            LastProcessedAt = blockchainState.LastProcessedAt
        };
    }

    private static BlockchainState MapEntityToDomain(BlockchainStateEntity entity)
    {
        return new BlockchainState(entity.LastProcessedHeight, entity.LastProcessedBlockHash, entity.LastProcessedAt)
        {
            Id = entity.Id
        };
    }
}