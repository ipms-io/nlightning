using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Transactions.Models;

namespace NLightning.Application.Bitcoin.Interfaces;

public interface ICommitmentTransactionBuilder
{
    SignedTransaction Build(CommitmentTransactionModel transaction);
}