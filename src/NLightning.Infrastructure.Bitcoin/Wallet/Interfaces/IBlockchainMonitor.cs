using NLightning.Domain.Bitcoin.Events;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Infrastructure.Bitcoin.Wallet.Interfaces;

public interface IBlockchainMonitor
{
    Task WatchTransactionAsync(ChannelId channelId, TxId txId, uint requiredDepth);
    event EventHandler<NewBlockEventArgs> OnNewBlockDetected;
    event EventHandler<TransactionConfirmedEventArgs> OnTransactionConfirmed;

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