namespace NLightning.Domain.Bitcoin.Interfaces;

using ValueObjects;

/// <summary>
/// Repository interface for managing blockchain state
/// </summary>
public interface IBlockchainStateDbRepository
{
    /// <summary>
    /// Adds a new blockchain state or updates the existing one in the repository.
    /// </summary>
    /// <param name="blockchainState">
    /// The blockchain state to be added or updated. This includes details such as the last processed
    /// block height, block hash, and the timestamp of the last processed block.
    /// </param>
    Task AddOrUpdateAsync(BlockchainState blockchainState);

    Task<BlockchainState?> GetStateAsync();
}