using NLightning.Domain.Channels.Models;
using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Channels.Interfaces;

public interface IChannelDbRepository
{
    Task AddAsync(ChannelModel channelModel);
    Task UpdateAsync(ChannelModel channelModel);
    Task DeleteAsync(ChannelId channelId);
    Task<ChannelModel?> GetByIdAsync(ChannelId channelId);
    Task<IEnumerable<ChannelModel>> GetAllAsync();
    Task<IEnumerable<ChannelModel>> GetReadyChannelsAsync();
}