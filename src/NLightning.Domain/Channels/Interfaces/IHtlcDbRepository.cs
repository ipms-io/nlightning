namespace NLightning.Domain.Channels.Interfaces;

using Enums;
using ValueObjects;

/// <summary>
/// Repository interface for managing HTLC (Hashed Time-Locked Contract) entities
/// </summary>
public interface IHtlcDbRepository
{
    /// <summary>
    /// Adds a new HTLC to a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="htlc">HTLC to add</param>
    Task AddAsync(ChannelId channelId, Htlc htlc);
    
    /// <summary>
    /// Updates an existing HTLC
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="htlc">Updated HTLC</param>
    Task UpdateAsync(ChannelId channelId, Htlc htlc);

    /// <summary>
    /// Deletes an HTLC
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="htlcId">HTLC ID</param>
    /// <param name="direction">HTLC direction (incoming/outgoing)</param>
    Task DeleteAsync(ChannelId channelId, ulong htlcId, HtlcDirection direction);
    
    /// <summary>
    /// Deletes all HTLCs for a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    void DeleteAllForChannelId(ChannelId channelId);

    /// <summary>
    /// Gets a specific HTLC
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="htlcId">HTLC ID</param>
    /// <param name="direction">HTLC direction (incoming/outgoing)</param>
    /// <returns>HTLC or null if not found</returns>
    Task<Htlc?> GetByIdAsync(ChannelId channelId, ulong htlcId, HtlcDirection direction);
    
    /// <summary>
    /// Gets all HTLCs for a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <returns>Collection of HTLCs</returns>
    Task<IEnumerable<Htlc>> GetAllForChannelAsync(ChannelId channelId);
    
    /// <summary>
    /// Gets HTLCs for a channel with a specific state
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="state">HTLC state</param>
    /// <returns>Collection of HTLCs matching the state</returns>
    Task<IEnumerable<Htlc>> GetByChannelIdAndStateAsync(ChannelId channelId, HtlcState state);
    
    /// <summary>
    /// Gets HTLCs for a channel with a specific direction
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="direction">HTLC direction (incoming/outgoing)</param>
    /// <returns>Collection of HTLCs matching the direction</returns>
    Task<IEnumerable<Htlc>> GetByChannelIdAndDirectionAsync(ChannelId channelId, HtlcDirection direction);
}
