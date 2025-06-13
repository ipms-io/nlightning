namespace NLightning.Domain.Bitcoin.Interfaces;

using ValueObjects;

/// <summary>
/// Repository interface for managing blockchain state
/// </summary>
public interface IBlockchainStateDbRepository
{
    /// <summary>
    /// Adds a blockchain state object to the repository.
    /// </summary>
    /// <param name="blockchainState">The blockchain state to add.</param>
    void Add(BlockchainState blockchainState);

    /// <summary>
    /// Updates an existing blockchain state in the repository.
    /// </summary>
    /// <param name="blockchainState">The blockchain state to update.</param>
    void Update(BlockchainState blockchainState);

    Task<BlockchainState?> GetStateAsync();
}