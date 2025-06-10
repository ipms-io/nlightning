using NLightning.Domain.Bitcoin.Transactions.Enums;
using NLightning.Domain.Bitcoin.Transactions.Models;
using NLightning.Domain.Channels.Models;

namespace NLightning.Domain.Bitcoin.Transactions.Interfaces;

/// <summary>
/// Interface for the factory that creates commitment transactions.
/// </summary>
public interface ICommitmentTransactionModelFactory
{
    /// <summary>
    /// Creates a domain model of a commitment transaction for the specified channel.
    /// </summary>
    /// <param name="channel">The channel for which to create the commitment transaction.</param>
    /// <param name="side">Whether to create a local or remote commitment transaction.</param>
    /// <returns>A domain model of the commitment transaction.</returns>
    CommitmentTransactionModel CreateCommitmentTransactionModel(ChannelModel channel,
                                                                CommitmentSide side);
}