using NLightning.Domain.Bitcoin.Events;
using NLightning.Domain.Bitcoin.ValueObjects;

namespace NLightning.Application.Bitcoin.Interfaces;

public interface IBlockchainMonitor
{
    Task WatchTransactionAsync(TxId txId, uint requiredDepth, Func<TxId, uint, Task> onConfirmed);
    Task WatchForRevocationAsync(TxId commitmentTxId, SignedTransaction penaltyTx);
    event EventHandler<NewBlockEventArgs> NewBlockDetected;

    /// <summary>
    /// Starts a background task to periodically refresh the fee rate
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the background task and cancels any ongoing operations within the service.
    /// </summary>
    /// <returns>A task representing the asynchronous stop operation.</returns>
    Task StopAsync();
}