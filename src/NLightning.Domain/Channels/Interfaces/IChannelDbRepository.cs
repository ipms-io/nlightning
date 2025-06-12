namespace NLightning.Domain.Channels.Interfaces;

using Models;
using ValueObjects;

public interface IChannelDbRepository
{
    Task AddAsync(ChannelModel channelModel);
    Task UpdateAsync(ChannelModel channelModel);
    Task<ChannelModel?> GetByIdAsync(ChannelId channelId);
    Task<IEnumerable<ChannelModel>> GetAllAsync();
    Task<IEnumerable<ChannelModel>> GetReadyChannelsAsync();
}