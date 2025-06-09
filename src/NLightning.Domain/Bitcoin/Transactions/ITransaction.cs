namespace NLightning.Domain.Bitcoin.Transactions;

using ValueObjects;

public interface ITransaction
{
    TxId TxId { get; }
    bool IsValid { get; }
}