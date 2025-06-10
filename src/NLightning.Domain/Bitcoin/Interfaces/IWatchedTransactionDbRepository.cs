namespace NLightning.Domain.Bitcoin.Interfaces;

using Transactions.Models;

public interface IWatchedTransactionDbRepository
{
    void Add(WatchedTransactionModel watchedTransactionModel);
    void Update(WatchedTransactionModel watchedTransactionModel);
    Task<IEnumerable<WatchedTransactionModel>> GetAllPendingAsync();
}