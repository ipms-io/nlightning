namespace NLightning.Infrastructure.Persistence.Entities.Bitcoin;

using Domain.Crypto.ValueObjects;

public class BlockchainStateEntity
{
    public required Guid Id { get; set; }
    public required uint LastProcessedHeight { get; set; }
    public required Hash LastProcessedBlockHash { get; set; }
    public required DateTime LastProcessedAt { get; set; }

    // Default constructor for EF Core
    internal BlockchainStateEntity() { }

    public override bool Equals(object? obj)
    {
        return obj is BlockchainStateEntity other && Equals(other);
    }

    public bool Equals(BlockchainStateEntity other)
    {
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}