namespace NLightning.Domain.Channels.Interfaces;

using ValueObjects;

/// <summary>
/// Repository interface for managing channel configurations
/// </summary>
public interface IChannelConfigDbRepository
{
    /// <summary>
    /// Adds a new channel configuration
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="config">Channel configuration</param>
    void Add(ChannelId channelId, ChannelConfig config);
    
    /// <summary>
    /// Updates an existing channel configuration
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="config">Updated channel configuration</param>
    void Update(ChannelId channelId, ChannelConfig config);
    
    /// <summary>
    /// Deletes a channel configuration
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    Task DeleteAsync(ChannelId channelId);
    
    /// <summary>
    /// Gets a channel configuration by channel ID
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <returns>Channel configuration or null if not found</returns>
    Task<ChannelConfig?> GetByChannelIdAsync(ChannelId channelId);
}