namespace NLightning.Domain.Bitcoin.Events;

using Transactions.Models;

public class TransactionConfirmedEventArgs : EventArgs
{
    public WatchedTransactionModel WatchedTransaction { get; }
    public uint Height { get; }

    public TransactionConfirmedEventArgs(WatchedTransactionModel watchedTransaction, uint height)
    {
        WatchedTransaction = watchedTransaction;
        Height = height;
    }
}