using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NetMQ;
using NetMQ.Sockets;
using NLightning.Domain.Bitcoin.Events;
using NLightning.Domain.Bitcoin.Transactions.Models;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Node.Options;
using NLightning.Domain.Persistence.Interfaces;
using NLightning.Infrastructure.Bitcoin.Options;
using NLightning.Infrastructure.Bitcoin.Wallet.Interfaces;

namespace NLightning.Infrastructure.Bitcoin.Wallet;

public class BlockchainMonitorService : IBlockchainMonitor
{
    private readonly BitcoinOptions _bitcoinOptions;
    private readonly IBitcoinWallet _bitcoinWallet;
    private readonly ILogger<BlockchainMonitorService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Network _network;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentDictionary<uint256, WatchedTransactionModel> _watchedTransactions = new();
    private readonly OrderedDictionary<uint, Block> _blocksToProcess = new();

    private BlockchainState _blockchainState = new(0, Hash.Empty, DateTime.UtcNow);
    private CancellationTokenSource? _cts;
    private Task? _monitoringTask;
    private SubscriberSocket? _blockSocket;
    // private SubscriberSocket? _transactionSocket;

    public event EventHandler<NewBlockEventArgs>? OnNewBlockDetected;
    public event EventHandler<TransactionConfirmedEventArgs>? OnTransactionConfirmed;

    public BlockchainMonitorService(IOptions<BitcoinOptions> bitcoinOptions, IBitcoinWallet bitcoinWallet,
                                    ILogger<BlockchainMonitorService> logger, IOptions<NodeOptions> nodeOptions,
                                    IServiceProvider serviceProvider)
    {
        _bitcoinOptions = bitcoinOptions.Value;
        _bitcoinWallet = bitcoinWallet;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _network = Network.GetNetwork(nodeOptions.Value.BitcoinNetwork) ?? Network.Main;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        using var scope = _serviceProvider.CreateScope();
        using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Load pending transactions
        await LoadPendingWatchedTransactionsAsync(uow);

        // Get the current state or create a new one if it doesn't exist
        _blockchainState = await uow.BlockchainStateDbRepository.GetStateAsync() ?? _blockchainState;
        if (_blockchainState.LastProcessedHeight == 0)
        {
            var lastProcessedHeight = await _bitcoinWallet.GetCurrentBlockHeightAsync();
            _logger.LogInformation("No blockchain state found, starting from height {Height}", lastProcessedHeight);

            _blockchainState = new BlockchainState(0, Hash.Empty, DateTime.UtcNow);
            await uow.BlockchainStateDbRepository.AddOrUpdateAsync(_blockchainState);
            await uow.SaveChangesAsync();
        }
        else
        {
            _logger.LogInformation("Starting blockchain monitoring at height {Height}, last block hash {LastBlockHash}",
                                   _blockchainState.LastProcessedHeight, _blockchainState.LastProcessedBlockHash);
        }

        // Process missing blocks
        var currentHeight = await _bitcoinWallet.GetCurrentBlockHeightAsync();
        await AddMissingBlocksToProcessAsync(currentHeight);
        if (_blocksToProcess.Count > 0)
        {
            await ProcessPendingBlocksAsync(uow);
        }

        await uow.SaveChangesAsync();

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

    public async Task WatchTransactionAsync(ChannelId channelId, TxId txId, uint requiredDepth)
    {
        _logger.LogInformation("Watching transaction {TxId} for {RequiredDepth} confirmations for channel {channelId}",
                               txId, requiredDepth, channelId);

        using var scope = _serviceProvider.CreateScope();
        using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var nBitcoinTxId = new uint256(txId);
        var watchedTx = new WatchedTransactionModel(channelId, txId, requiredDepth);

        uow.WatchedTransactionDbRepository.Add(watchedTx);

        _watchedTransactions[nBitcoinTxId] = watchedTx;

        await uow.SaveChangesAsync();
    }

    // public Task WatchForRevocationAsync(TxId commitmentTxId, SignedTransaction penaltyTx)
    // {
    //     _logger.LogInformation("Watching for revocation of commitment transaction {CommitmentTxId}", commitmentTxId);
    //
    //     var nBitcoinTxId = new uint256(commitmentTxId);
    //     var revocationWatch = new RevocationWatch(nBitcoinTxId, Transaction.Load(penaltyTx.RawTxBytes, _network));
    //
    //     _revocationWatches.TryAdd(nBitcoinTxId, revocationWatch);
    //     return Task.CompletedTask;
    // }

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
                            var currentHeight = await _bitcoinWallet.GetCurrentBlockHeightAsync();
                            var block = Block.Load(blockHashBytes, _network);
                            await ProcessNewBlock(block, currentHeight);
                        }
                    }

                    // TODO: Check for new transactions
                    // if (_transactionSocket != null &&
                    //     _transactionSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(100), out var txTopic))
                    // {
                    //     if (txTopic == "rawtx" && _transactionSocket.TryReceiveFrameBytes(out var rawTxBytes))
                    //     {
                    //         await ProcessNewTransaction(rawTxBytes);
                    //     }
                    // }

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

            // // Subscribe to new transactions (for mempool monitoring)
            // _transactionSocket = new SubscriberSocket();
            // _transactionSocket.Connect($"tcp://{_bitcoinOptions.ZmqHost}:{_bitcoinOptions.ZmqTxPort}");
            // _transactionSocket.Subscribe("rawtx");

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

            // _transactionSocket?.Dispose();
            // _transactionSocket = null;

            _logger.LogDebug("ZMQ sockets cleaned up");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up ZMQ sockets");
        }
    }

    private async Task ProcessPendingBlocksAsync(IUnitOfWork uow)
    {
        await _semaphore.WaitAsync();

        try
        {
            while (_blocksToProcess.Count > 0)
            {
                var blockKvp = _blocksToProcess.First();
                if (blockKvp.Key < _blockchainState.LastProcessedHeight)
                {
                    // TODO: Maybe we had a reorg?
                    _logger.LogWarning("Possible reorg detected: Block {Height} is already processed", blockKvp.Key);
                    _blocksToProcess.Remove(blockKvp.Key);
                }

                ProcessBlock(blockKvp.Value, blockKvp.Key, uow);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task AddMissingBlocksToProcessAsync(uint currentHeight)
    {
        if (currentHeight > _blockchainState.LastProcessedHeight + 1)
        {
            _logger.LogWarning("Processing missed blocks from height {LastProcessedHeight} to {CurrentHeight}",
                               _blockchainState.LastProcessedHeight, currentHeight);

            for (var height = _blockchainState.LastProcessedHeight + 1; height < currentHeight; height++)
            {
                if (_blocksToProcess.ContainsKey(height))
                    continue;

                // Add missing block to process queue
                var blockAtHeight = await _bitcoinWallet.GetBlockAsync(height);
                if (blockAtHeight is not null)
                {
                    _blocksToProcess[height] = blockAtHeight;
                }
                else
                {
                    _logger.LogError("Missing block at height {Height}", height);
                }
            }
        }
    }

    private async Task ProcessNewBlock(Block block, uint currentHeight)
    {
        using var scope = _serviceProvider.CreateScope();
        using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var blockHash = block.GetHash();

        try
        {
            _logger.LogDebug("We have a new block at height {blockHeight}: {BlockHash}", currentHeight, blockHash);

            // Check for missed blocks first
            await AddMissingBlocksToProcessAsync(currentHeight);

            // Store the current block for processing
            _blocksToProcess[currentHeight] = block;

            // Process missing blocks
            await ProcessPendingBlocksAsync(uow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing new block {BlockHash}", blockHash);
        }

        await uow.SaveChangesAsync();
    }

    // TODO: Check for revocation transactions in mempool
    // private async Task ProcessNewTransaction(byte[] rawTxBytes)
    // {
    //     try
    //     {
    //         var transaction = Transaction.Load(rawTxBytes, Network.Main);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error processing new transaction from mempool");
    //     }
    // }

    private void ProcessBlock(Block block, uint height, IUnitOfWork uow)
    {
        try
        {
            var blockHash = block.GetHash();

            _logger.LogDebug("Processing block {Height} with {TxCount} transactions", height, block.Transactions.Count);

            // Notify listeners of the new block
            OnNewBlockDetected?.Invoke(this, new NewBlockEventArgs(height, blockHash.ToBytes()));

            // Check if watched transactions are included in this block
            CheckWatchedTransactionsForBlock(block.Transactions, height, uow);

            // Update blockchain state
            _blockchainState.UpdateState(blockHash.ToBytes());
            uow.BlockchainStateDbRepository.AddOrUpdateAsync(_blockchainState);

            // Check watched for all transactions' depth
            CheckWatchedTransactionsDepth(uow);

            _blocksToProcess.Remove(height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing block at height {Height}", height);
        }
    }

    private void ConfirmTransaction(uint blockHeight, IUnitOfWork uow, WatchedTransactionModel watchedTransaction)
    {
        _logger.LogInformation(
            "Transaction {TxId} reached required depth of {depth} confirmations at block {blockHeight}",
            watchedTransaction.TransactionId, watchedTransaction.RequiredDepth, blockHeight);

        watchedTransaction.MarkAsCompleted();
        uow.WatchedTransactionDbRepository.Update(watchedTransaction);
        OnTransactionConfirmed?.Invoke(
            this, new TransactionConfirmedEventArgs(watchedTransaction, blockHeight));

        _watchedTransactions.TryRemove(new uint256(watchedTransaction.TransactionId), out _);
    }

    private void CheckWatchedTransactionsForBlock(List<Transaction> blockTransactions, uint blockHeight,
                                                  IUnitOfWork uow)
    {
        _logger.LogDebug("Checking watched transactions for block {height} with {TxCount} transactions", blockHeight,
                         blockTransactions.Count);

        foreach (var transaction in blockTransactions)
        {
            var txId = transaction.GetHash();

            if (!_watchedTransactions.TryGetValue(txId, out var watchedTransaction))
                continue;

            _logger.LogInformation("Transaction {TxId} found in block at height {Height}", txId, blockHeight);

            try
            {
                // Update first seen height
                watchedTransaction.SetFirstSeenAtHeight(blockHeight);
                uow.WatchedTransactionDbRepository.Update(watchedTransaction);

                if (watchedTransaction.RequiredDepth == 0)
                    ConfirmTransaction(blockHeight, uow, watchedTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking confirmations for transaction {TxId}", txId);
            }
        }
    }

    private void CheckWatchedTransactionsDepth(IUnitOfWork uow)
    {
        foreach (var (txId, watchedTransaction) in _watchedTransactions)
        {
            try
            {
                var confirmations = _blockchainState.LastProcessedHeight - watchedTransaction.FirstSeenAtHeight;
                if (confirmations >= watchedTransaction.RequiredDepth)
                    ConfirmTransaction(_blockchainState.LastProcessedHeight, uow, watchedTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking confirmations for transaction {TxId}", txId);
            }
        }
    }

    private async Task LoadPendingWatchedTransactionsAsync(IUnitOfWork uow)
    {
        _logger.LogInformation("Loading watched transactions from database");

        var watchedTransactions = await uow.WatchedTransactionDbRepository.GetAllPendingAsync();
        foreach (var watchedTransaction in watchedTransactions)
        {
            _watchedTransactions[new uint256(watchedTransaction.TransactionId)] = watchedTransaction;
        }
    }
}