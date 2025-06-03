using NLightning.Domain.Channels.Models;
using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Channels.Interfaces;

public interface IChannelDbRepository
{
    Task AddAsync(Channel channel);
    Task UpdateAsync(Channel channel);
    Task DeleteAsync(ChannelId channelId);
    Task<Channel?> GetByIdAsync(ChannelId channelId);
    Task<IEnumerable<Channel>> GetAllAsync();
    Task<IEnumerable<Channel>> GetReadyChannelsAsync();
    
}