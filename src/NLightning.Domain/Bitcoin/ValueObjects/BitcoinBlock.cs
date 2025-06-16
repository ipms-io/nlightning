using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Bitcoin.ValueObjects;

public record BitcoinBlock
{
    public byte[] BlockData { get; }
    public int TransactionCount { get; }
    public Hash BlockHash { get; }

    public BitcoinBlock(byte[] blockData, Hash blockHash, int transactionCount)
    {
        BlockData = blockData;
        BlockHash = blockHash;
        TransactionCount = transactionCount;
    }
}