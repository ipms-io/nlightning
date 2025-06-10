using NLightning.Domain.Bitcoin.Interfaces;

namespace NLightning.Domain.Persistence.Interfaces;

using Channels.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Bitcoin repositories
    IBlockchainStateDbRepository BlockchainStateDbRepository { get; }
    IWatchedTransactionDbRepository WatchedTransactionDbRepository { get; }

    // Chanel repositories
    IChannelConfigDbRepository ChannelConfigDbRepository { get; }
    IChannelDbRepository ChannelDbRepository { get; }
    IChannelKeySetDbRepository ChannelKeySetDbRepository { get; }
    IHtlcDbRepository HtlcDbRepository { get; }

    void SaveChanges();
    Task SaveChangesAsync();
}