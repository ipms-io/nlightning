using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Bitcoin.Transactions.Models;

public class WatchedTransactionModel
{
    public ChannelId ChannelId { get; }
    public TxId TransactionId { get; }
    public uint RequiredDepth { get; }
    public uint? FirstSeenAtHeight { get; private set; }
    public bool IsCompleted { get; private set; }

    public WatchedTransactionModel(ChannelId channelId, TxId transactionId, uint requiredDepth)
    {
        ChannelId = channelId;
        TransactionId = transactionId;
        RequiredDepth = requiredDepth;
    }

    public void SetFirstSeenAtHeight(uint height)
    {
        if (FirstSeenAtHeight.HasValue)
            throw new InvalidOperationException("FirstSeenAtHeight is already set.");

        FirstSeenAtHeight = height;
    }

    public void MarkAsCompleted()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Transaction is already marked as completed.");

        IsCompleted = true;
    }
}