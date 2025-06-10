namespace NLightning.Infrastructure.Persistence.Entities.Bitcoin;

using Domain.Bitcoin.ValueObjects;
using Domain.Channels.ValueObjects;

public class WatchedTransactionEntity
{
    public required ChannelId ChannelId { get; set; }
    public required TxId TransactionId { get; set; }
    public required uint RequiredDepth { get; set; }
    public uint? FirstSeenAtHeight { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Default constructor for EF Core
    internal WatchedTransactionEntity()
    {
    }
}