using NLightning.Domain.Transactions.Models;

namespace NLightning.Domain.Transactions.Interfaces;

using Channels.Models;
using Enums;

/// <summary>
/// Interface for the factory that creates commitment transactions.
/// </summary>
public interface ICommitmentTransactionModelFactory
{
    /// <summary>
    /// Creates a domain model of a commitment transaction for the specified channel.
    /// </summary>
    /// <param name="channel">The channel for which to create the commitment transaction.</param>
    /// <param name="commitmentSide">Whether to create a local or remote commitment transaction.</param>
    /// <returns>A domain model of the commitment transaction.</returns>
    CommitmentTransactionModel CreateCommitmentTransactionModel(Channel channel, CommitmentSide commitmentSide);
}
