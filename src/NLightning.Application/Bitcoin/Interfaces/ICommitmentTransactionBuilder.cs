namespace NLightning.Application.Bitcoin.Interfaces;

using Domain.Bitcoin.ValueObjects;
using Domain.Transactions.Models;

public interface ICommitmentTransactionBuilder
{
    SignedTransaction Build(CommitmentTransactionModel transaction);
}