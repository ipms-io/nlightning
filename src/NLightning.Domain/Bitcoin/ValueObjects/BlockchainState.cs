namespace NLightning.Domain.Bitcoin.ValueObjects;

using Crypto.ValueObjects;

public record BlockchainState
{
    public uint LastProcessedHeight { get; private set; }
    public Hash LastProcessedBlockHash { get; private set; }
    public DateTime LastProcessedAt { get; private set; }

    public BlockchainState(uint lastProcessedHeight, Hash lastProcessedBlockHash, DateTime lastProcessedAt)
    {
        LastProcessedHeight = lastProcessedHeight;
        LastProcessedBlockHash = lastProcessedBlockHash;
        LastProcessedAt = lastProcessedAt;
    }

    public void UpdateState(Hash newBlockHash)
    {
        LastProcessedHeight++;
        LastProcessedBlockHash = newBlockHash;
        LastProcessedAt = DateTime.UtcNow;
    }
}