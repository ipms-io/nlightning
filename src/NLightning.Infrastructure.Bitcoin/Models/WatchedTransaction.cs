using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Models;

internal class WatchedTransaction
{
    public uint256 TransactionId { get; init; }
    public uint RequiredDepth { get; init; }
    public Func<uint256, uint, Task> OnConfirmed { get; init; }

    public WatchedTransaction(uint256 transactionId, uint requiredDepth, Func<uint256, uint, Task> onConfirmed)
    {
        TransactionId = transactionId;
        RequiredDepth = requiredDepth;
        OnConfirmed = onConfirmed;
    }
}