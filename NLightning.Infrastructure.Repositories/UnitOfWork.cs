using NLightning.Domain.Bitcoin.Interfaces;
using NLightning.Domain.Channels.Interfaces;
using NLightning.Domain.Crypto.Hashes;
using NLightning.Domain.Persistence.Interfaces;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Serialization.Interfaces;
using NLightning.Infrastructure.Persistence.Contexts;
using NLightning.Infrastructure.Repositories.Database.Channels;

namespace NLightning.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NLightningDbContext _context;
        private readonly IMessageSerializer _messageSerializer;
        private readonly ISecretStorageServiceFactory _secretStorageServiceFactory;
        private readonly ISha256 _sha256;

        private ChannelConfigDbRepository? _channelConfigDbRepository;
        private ChannelDbRepository? _channelDbRepository;
        private ChannelKeySetDbRepository? _channelKeySetDbRepository;
        private HtlcDbRepository? _htlcDbRepository;
        
        public IChannelConfigDbRepository ChannelConfigDbRepository => 
            _channelConfigDbRepository ??= new ChannelConfigDbRepository(_context);
        
        public IChannelDbRepository ChannelDbRepository => 
            _channelDbRepository ??= new ChannelDbRepository(_context, _messageSerializer, _secretStorageServiceFactory,
                                                             _sha256);
        
        public IChannelKeySetDbRepository ChannelKeySetDbRepository =>
            _channelKeySetDbRepository ??= new ChannelKeySetDbRepository(_context, _secretStorageServiceFactory);
        
        public IHtlcDbRepository HtlcDbRepository => 
            _htlcDbRepository ??= new HtlcDbRepository(_context, _messageSerializer);
        
        public UnitOfWork(NLightningDbContext context,
                          IMessageSerializer messageSerializer,
                          ISecretStorageServiceFactory secretStorageServiceFactory, ISha256 sha256)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _messageSerializer = messageSerializer;
            _secretStorageServiceFactory = secretStorageServiceFactory;
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
}

