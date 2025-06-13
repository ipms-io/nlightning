namespace NLightning.Domain.Bitcoin.ValueObjects;

using Crypto.ValueObjects;

public record BlockchainState
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Hash LastProcessedBlockHash { get; private set; }
    public uint LastProcessedHeight { get; private set; }
    public DateTime LastProcessedAt { get; private set; }

    public BlockchainState(uint lastProcessedHeight, Hash lastProcessedBlockHash, DateTime lastProcessedAt)
    {
        LastProcessedHeight = lastProcessedHeight;
        LastProcessedBlockHash = lastProcessedBlockHash;
        LastProcessedAt = lastProcessedAt;
    }

    public void UpdateState(Hash newBlockHash, uint newBlockHeight)
    {
        if (newBlockHeight < LastProcessedHeight)
            throw new InvalidOperationException("New block height cannot be lower than the last processed height.");

        LastProcessedBlockHash = newBlockHash;
        LastProcessedHeight = newBlockHeight;
        LastProcessedAt = DateTime.UtcNow;
    }
}