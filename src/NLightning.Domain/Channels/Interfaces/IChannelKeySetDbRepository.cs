namespace NLightning.Domain.Channels.Interfaces;

using Models;
using ValueObjects;

/// <summary>
/// Repository interface for managing channel key sets
/// </summary>
public interface IChannelKeySetDbRepository
{
    /// <summary>
    /// Adds a new channel key set
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="isLocal">True if this is the local key set, false for remote</param>
    /// <param name="keySetModel">The key set to add</param>
    void Add(ChannelId channelId, bool isLocal, ChannelKeySetModel keySetModel);

    /// <summary>
    /// Updates an existing channel key set
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="isLocal">True if this is the local key set, false for remote</param>
    /// <param name="keySetModel">The updated key set</param>
    void Update(ChannelId channelId, bool isLocal, ChannelKeySetModel keySetModel);

    /// <summary>
    /// Deletes a channel key set
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="isLocal">True if this is the local key set, false for remote</param>
    Task DeleteAsync(ChannelId channelId, bool isLocal);

    /// <summary>
    /// Gets a channel key set
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="isLocal">True if this is the local key set, false for remote</param>
    /// <returns>Channel key set or null if not found</returns>
    Task<ChannelKeySetModel?> GetByIdAsync(ChannelId channelId, bool isLocal);
}