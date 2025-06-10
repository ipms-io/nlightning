namespace NLightning.Infrastructure.Persistence.Entities.Bitcoin;

using Domain.Crypto.ValueObjects;

public class BlockchainStateEntity
{
    public required Guid Id { get; set; }
    public required uint LastProcessedHeight { get; set; }
    public required Hash LastProcessedBlockHash { get; set; }
    public required DateTime LastProcessedAt { get; set; }

    // Default constructor for EF Core
    internal BlockchainStateEntity()
    {
    }
}