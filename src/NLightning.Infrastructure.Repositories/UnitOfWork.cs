namespace NLightning.Infrastructure.Repositories;

using Database.Bitcoin;
using Database.Channel;
using Domain.Bitcoin.Interfaces;
using Domain.Channels.Interfaces;
using Domain.Crypto.Hashes;
using Domain.Persistence.Interfaces;
using Domain.Serialization.Interfaces;
using Persistence.Contexts;

public class UnitOfWork : IUnitOfWork
{
    private readonly NLightningDbContext _context;
    private readonly IMessageSerializer _messageSerializer;
    private readonly ISha256 _sha256;

    // Bitcoin repositories
    private BlockchainStateDbRepository? _blockchainStateDbRepository;
    private WatchedTransactionDbRepository? _watchedTransactionDbRepository;

    // Channel repositories
    private ChannelConfigDbRepository? _channelConfigDbRepository;
    private ChannelDbRepository? _channelDbRepository;
    private ChannelKeySetDbRepository? _channelKeySetDbRepository;
    private HtlcDbRepository? _htlcDbRepository;

    public IBlockchainStateDbRepository BlockchainStateDbRepository =>
        _blockchainStateDbRepository ??= new BlockchainStateDbRepository(_context);

    public IWatchedTransactionDbRepository WatchedTransactionDbRepository =>
        _watchedTransactionDbRepository ??= new WatchedTransactionDbRepository(_context);

    public IChannelConfigDbRepository ChannelConfigDbRepository =>
        _channelConfigDbRepository ??= new ChannelConfigDbRepository(_context);

    public IChannelDbRepository ChannelDbRepository =>
        _channelDbRepository ??= new ChannelDbRepository(_context, _messageSerializer, _sha256);

    public IChannelKeySetDbRepository ChannelKeySetDbRepository =>
        _channelKeySetDbRepository ??= new ChannelKeySetDbRepository(_context);

    public IHtlcDbRepository HtlcDbRepository =>
        _htlcDbRepository ??= new HtlcDbRepository(_context, _messageSerializer);

    public UnitOfWork(NLightningDbContext context, IMessageSerializer messageSerializer, ISha256 sha256)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _messageSerializer = messageSerializer;
        _sha256 = sha256;
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    #region Dispose Pattern

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                _context.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}