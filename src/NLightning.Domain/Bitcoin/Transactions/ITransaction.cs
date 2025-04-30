using NBitcoin;

namespace NLightning.Domain.Bitcoin.Transactions;

public interface ITransaction
{
    uint256 TxId { get; }
    bool IsValid { get; }
}