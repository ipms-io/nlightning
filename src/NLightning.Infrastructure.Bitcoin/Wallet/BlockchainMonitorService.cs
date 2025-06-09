using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NetMQ;
using NetMQ.Sockets;
using NLightning.Application.Bitcoin.Interfaces;
using NLightning.Domain.Bitcoin.Events;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Node.Options;
using NLightning.Infrastructure.Bitcoin.Models;
using NLightning.Infrastructure.Bitcoin.Options;
using NLightning.Infrastructure.Bitcoin.Wallet.Interfaces;

namespace NLightning.Infrastructure.Bitcoin.Wallet;

public class BlockchainMonitorService : IBlockchainMonitor
{
    private readonly BitcoinOptions _bitcoinOptions;
    private readonly IBitcoinWallet _bitcoinWallet;
    private readonly ILogger<BlockchainMonitorService> _logger;
    private readonly Network _network;
    private readonly ConcurrentDictionary<uint256, WatchedTransaction> _watchedTransactions = new();
    private readonly ConcurrentDictionary<uint256, RevocationWatch> _revocationWatches = new();

    private uint _lastProcessedHeight;
    private CancellationTokenSource? _cts;
    private Task? _monitoringTask;
    private SubscriberSocket? _blockSocket;
    private SubscriberSocket? _transactionSocket;

    public event EventHandler<NewBlockEventArgs>? NewBlockDetected;

    public BlockchainMonitorService(IOptions<BitcoinOptions> bitcoinOptions, IBitcoinWallet bitcoinWallet,
                                    ILogger<BlockchainMonitorService> logger,
                                    IOptions<NodeOptions> nodeOptions)
    {
        _bitcoinOptions = bitcoinOptions.Value;
        _bitcoinWallet = bitcoinWallet;
        _logger = logger;
        _network = Network.GetNetwork(nodeOptions.Value.BitcoinNetwork) ?? Network.Main;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _lastProcessedHeight = await _bitcoinWallet.GetCurrentBlockHeightAsync();
        _logger.LogInformation("Starting blockchain monitoring at height {Height}", _lastProcessedHeight);

        // Initialize ZMQ sockets
        InitializeZmqSockets();

        // Start monitoring task
        _monitoringTask = MonitorBlockchainAsync(_cts.Token);

        _logger.LogInformation("Blockchain monitor service started successfully");
    }

    public async Task StopAsync()
    {
        if (_cts is null)
        {
            throw new InvalidOperationException("Service is not running");
        }

        await _cts.CancelAsync();

        if (_monitoringTask is not null)
        {
            try
            {
                await _monitoringTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during cancellation
            }
        }

        CleanupZmqSockets();
    }

    public Task WatchTransactionAsync(TxId txId, uint requiredDepth, Func<TxId, uint, Task> onConfirmed)
    {
        _logger.LogInformation("Watching transaction {TxId} for {RequiredDepth} confirmations", txId, requiredDepth);

        var nBitcoinTxId = new uint256(txId);
        var watchedTx = new WatchedTransaction(nBitcoinTxId, requiredDepth,
                                               (transactionId, reqDepth) =>
                                                   onConfirmed(transactionId.ToBytes(), reqDepth));

        _watchedTransactions.TryAdd(nBitcoinTxId, watchedTx);
        return Task.CompletedTask;
    }

    public Task WatchForRevocationAsync(TxId commitmentTxId, SignedTransaction penaltyTx)
    {
        _logger.LogInformation("Watching for revocation of commitment transaction {CommitmentTxId}", commitmentTxId);

        var nBitcoinTxId = new uint256(commitmentTxId);
        var revocationWatch = new RevocationWatch(nBitcoinTxId, Transaction.Load(penaltyTx.RawTxBytes, _network));

        _revocationWatches.TryAdd(nBitcoinTxId, revocationWatch);
        return Task.CompletedTask;
    }

    private async Task MonitorBlockchainAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting blockchain monitoring loop");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Check for new blocks
                    if (_blockSocket != null &&
                        _blockSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(100), out var topic))
                    {
                        if (topic == "rawblock" && _blockSocket.TryReceiveFrameBytes(out var blockHashBytes))
                        {
                            var block = Block.Load(blockHashBytes, _network);
                            await ProcessNewBlock(block);
                        }
                    }

                    // Check for new transactions
                    if (_transactionSocket != null &&
                        _transactionSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(100), out var txTopic))
                    {
                        if (txTopic == "rawtx" && _transactionSocket.TryReceiveFrameBytes(out var rawTxBytes))
                        {
                            await ProcessNewTransaction(rawTxBytes);
                        }
                    }

                    // Periodically check watched transactions for confirmations
                    await CheckWatchedTransactions();

                    // Small delay to prevent CPU spinning
                    await Task.Delay(50, cancellationToken);
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error in blockchain monitoring loop");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Blockchain monitoring loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in blockchain monitoring loop");
        }
    }

    private void InitializeZmqSockets()
    {
        try
        {
            // Subscribe to new blocks
            _blockSocket = new SubscriberSocket();
            _blockSocket.Connect($"tcp://{_bitcoinOptions.ZmqHost}:{_bitcoinOptions.ZmqBlockPort}");
            _blockSocket.Subscribe("rawblock");

            // Subscribe to new transactions (for mempool monitoring)
            _transactionSocket = new SubscriberSocket();
            _transactionSocket.Connect($"tcp://{_bitcoinOptions.ZmqHost}:{_bitcoinOptions.ZmqTxPort}");
            _transactionSocket.Subscribe("rawtx");

            _logger.LogInformation("ZMQ sockets initialized - Block: {BlockPort}, Tx: {TxPort}",
                                   _bitcoinOptions.ZmqBlockPort, _bitcoinOptions.ZmqTxPort);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ZMQ sockets");
            CleanupZmqSockets();
            throw;
        }
    }

    private void CleanupZmqSockets()
    {
        try
        {
            _blockSocket?.Dispose();
            _blockSocket = null;

            _transactionSocket?.Dispose();
            _transactionSocket = null;

            _logger.LogDebug("ZMQ sockets cleaned up");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up ZMQ sockets");
        }
    }

    private async Task ProcessNewBlock(Block block)
    {
        var blockHash = block.GetHash();

        try
        {
            _logger.LogDebug("Processing new block {BlockHash}", blockHash);

            var currentHeight = await _bitcoinWallet.GetCurrentBlockHeightAsync();

            // Process any missed blocks first
            if (currentHeight > _lastProcessedHeight + 1)
            {
                for (var height = _lastProcessedHeight + 1; height < currentHeight; height++)
                {
                    await ProcessBlock(block, height);
                }
            }

            // Process the current block
            var currentBlock = await _bitcoinWallet.GetBlockAsync(currentHeight);
            if (currentBlock != null && currentBlock.GetHash() == blockHash)
            {
                await ProcessBlock(currentBlock, currentHeight);
                _lastProcessedHeight = currentHeight;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing new block {BlockHash}", blockHash);
        }
    }

    private async Task ProcessNewTransaction(byte[] rawTxBytes)
    {
        try
        {
            var transaction = Transaction.Load(rawTxBytes, Network.Main); // Use appropriate network
            await CheckRevocationAttempts([transaction]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing new transaction from mempool");
        }
    }

    private async Task ProcessBlock(Block block, uint height)
    {
        try
        {
            _logger.LogDebug("Processing block {Height} with {TxCount} transactions", height, block.Transactions.Count);

            // Notify listeners of the new block
            NewBlockDetected?.Invoke(this, new NewBlockEventArgs(height, block.GetHash().ToBytes()));

            // Check for revocation attempts in this block
            await CheckRevocationAttempts(block.Transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing block at height {Height}", height);
        }
    }

    private async Task CheckWatchedTransactions()
    {
        var completedWatches = new List<uint256>();

        foreach (var kvp in _watchedTransactions)
        {
            var txId = kvp.Key;
            var watch = kvp.Value;

            try
            {
                var confirmations = await _bitcoinWallet.GetTransactionConfirmationsAsync(txId);

                if (confirmations >= watch.RequiredDepth)
                {
                    _logger.LogInformation("Transaction {TxId} reached required depth {RequiredDepth} confirmations",
                                           txId, watch.RequiredDepth);

                    await watch.OnConfirmed(txId, confirmations);
                    completedWatches.Add(txId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking confirmations for transaction {TxId}", txId);
            }
        }

        // Clean up completed watches
        foreach (var txId in completedWatches)
        {
            _watchedTransactions.TryRemove(txId, out _);
        }
    }

    private async Task CheckRevocationAttempts(List<Transaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            foreach (var input in transaction.Inputs)
            {
                var prevTxId = input.PrevOut.Hash;

                if (_revocationWatches.TryGetValue(prevTxId, out var revocationWatch))
                {
                    _logger.LogWarning(
                        "Detected revocation attempt! Old commitment transaction {CommitmentTxId} was spent",
                        prevTxId);

                    try
                    {
                        // Broadcast penalty transaction
                        var penaltyTxId = await _bitcoinWallet.SendTransactionAsync(revocationWatch.PenaltyTransaction);
                        _logger.LogInformation("Successfully broadcast penalty transaction {PenaltyTxId}", penaltyTxId);

                        // Remove the watch since we've handled it
                        _revocationWatches.TryRemove(prevTxId, out _);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to broadcast penalty transaction for {CommitmentTxId}", prevTxId);
                    }
                }
            }
        }
    }
}