namespace NLightning.Domain.Channels.Repositories;

using ValueObjects;

public interface IChannelKeyRepository
{
    Task<ChannelKeyData?> GetByKeyIndexForChannelAsync(ChannelId channelId, CancellationToken ct = default);
}