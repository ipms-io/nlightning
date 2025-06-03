using NLightning.Domain.Channels.Interfaces;

namespace NLightning.Domain.Persistence.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IChannelConfigDbRepository ChannelConfigDbRepository { get; }
    IChannelDbRepository ChannelDbRepository { get; }
    IChannelKeySetDbRepository ChannelKeySetDbRepository { get; }
    IHtlcDbRepository HtlcDbRepository { get; }
    
    void SaveChanges();
    Task SaveChangesAsync();
}

