namespace NLightning.Infrastructure.Bitcoin.Builders.Interfaces;

using Domain.Bitcoin.Transactions.Models;
using Domain.Bitcoin.ValueObjects;

public interface ICommitmentTransactionBuilder
{
    SignedTransaction Build(CommitmentTransactionModel transaction);
}