using NLightning.Domain.Bitcoin.ValueObjects;

namespace NLightning.Domain.Bitcoin.Transactions;

public interface ITransaction
{
    TxId TxId { get; }
    bool IsValid { get; }
}