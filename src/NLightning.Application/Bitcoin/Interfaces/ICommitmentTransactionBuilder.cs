using NLightning.Domain.Bitcoin.Transactions;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Transactions.Models;

namespace NLightning.Application.Bitcoin.Interfaces;

public interface ICommitmentTransactionBuilder
{
    SignedTransaction Build(CommitmentTransactionModel transaction);
}