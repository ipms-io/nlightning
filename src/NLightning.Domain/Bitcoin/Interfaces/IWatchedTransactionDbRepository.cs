namespace NLightning.Domain.Bitcoin.Interfaces;

using Transactions.Models;
using ValueObjects;

public interface IWatchedTransactionDbRepository
{
    void Add(WatchedTransactionModel watchedTransactionModel);
    void Update(WatchedTransactionModel watchedTransactionModel);
    Task<IEnumerable<WatchedTransactionModel>> GetAllPendingAsync();
    Task<WatchedTransactionModel?> GetByTransactionIdAsync(TxId transactionId);
}