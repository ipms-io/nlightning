using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NLightning.Domain.Bitcoin.Interfaces;
using NLightning.Domain.Bitcoin.Transactions.Models;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Persistence.Interfaces;
using NLightning.Infrastructure.Bitcoin.Options;
using NLightning.Infrastructure.Bitcoin.Wallet;
using NLightning.Infrastructure.Bitcoin.Wallet.Interfaces;
using NLightning.Tests.Utils.Mocks;

namespace NLightning.Infrastructure.Tests.Bitcoin.Wallet;

public class BlockchainMonitorServiceTests
{
    private readonly Mock<IOptions<BitcoinOptions>> _mockBitcoinOptions;
    private readonly Mock<IBitcoinWallet> _mockBitcoinWallet;
    private readonly Mock<ILogger<BlockchainMonitorService>> _mockLogger;
    private readonly Mock<IOptions<Domain.Node.Options.NodeOptions>> _mockNodeOptions;
    private readonly FakeServiceProvider _fakeServiceProvider;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IBlockchainStateDbRepository> _mockBlockchainStateRepository;
    private readonly Mock<IWatchedTransactionDbRepository> _mockWatchedTransactionRepository;

    private readonly BlockchainMonitorService _service;

    public BlockchainMonitorServiceTests()
    {
        // Set up mock dependencies
        _mockBitcoinOptions = new Mock<IOptions<BitcoinOptions>>();
        _mockBitcoinOptions.Setup(x => x.Value).Returns(new BitcoinOptions
        {
            RpcEndpoint = "",
            RpcUser = "",
            RpcPassword = "",
            ZmqHost = "127.0.0.1",
            ZmqBlockPort = 28332,
            ZmqTxPort = 28333
        });

        _mockBitcoinWallet = new Mock<IBitcoinWallet>();
        _mockLogger = new Mock<ILogger<BlockchainMonitorService>>();

        _mockNodeOptions = new Mock<IOptions<Domain.Node.Options.NodeOptions>>();
        _mockNodeOptions.Setup(x => x.Value).Returns(new Domain.Node.Options.NodeOptions
        {
            BitcoinNetwork = "regtest"
        });

        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _fakeServiceProvider = new FakeServiceProvider();
        _fakeServiceProvider.AddService(typeof(IUnitOfWork), _mockUnitOfWork.Object);
        _mockBlockchainStateRepository = new Mock<IBlockchainStateDbRepository>();
        _mockWatchedTransactionRepository = new Mock<IWatchedTransactionDbRepository>();

        // Set up unit of work to return repositories
        _mockUnitOfWork.Setup(x => x.BlockchainStateDbRepository).Returns(_mockBlockchainStateRepository.Object);
        _mockUnitOfWork.Setup(x => x.WatchedTransactionDbRepository).Returns(_mockWatchedTransactionRepository.Object);

        // Create the service
        _service = new BlockchainMonitorService(
            _mockBitcoinOptions.Object,
            _mockBitcoinWallet.Object,
            _mockLogger.Object,
            _mockNodeOptions.Object,
            _fakeServiceProvider);
    }

    [Fact]
    public async Task StartAsync_WithExistingBlockchainState_LoadsStateAndPendingTransactions()
    {
        // Arrange
        var state = new BlockchainState(100, new byte[32], DateTime.UtcNow);
        var pendingTransactions = new List<Domain.Bitcoin.Transactions.Models.WatchedTransactionModel>
        {
            new(new ChannelId(new byte[32]), new TxId(new byte[32]), 6)
        };

        _mockBlockchainStateRepository.Setup(x => x.GetStateAsync())
                                      .ReturnsAsync(state);

        _mockWatchedTransactionRepository.Setup(x => x.GetAllPendingAsync())
                                         .ReturnsAsync(pendingTransactions);

        _mockBitcoinWallet.Setup(x => x.GetCurrentBlockHeightAsync())
                          .ReturnsAsync(110u);

        _mockBitcoinWallet.Setup(x => x.GetBlockAsync(It.IsAny<uint>()))
                          .ReturnsAsync(new Block());

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _mockBlockchainStateRepository.Verify(x => x.GetStateAsync(), Times.Once);
        _mockWatchedTransactionRepository.Verify(x => x.GetAllPendingAsync(), Times.Once);
        _mockBitcoinWallet.Verify(x => x.GetCurrentBlockHeightAsync(), Times.Once);
        _mockBitcoinWallet.Verify(x => x.GetBlockAsync(100), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithNoBlockchainState_CreatesNewState()
    {
        // Arrange
        _mockBlockchainStateRepository.Setup(x => x.GetStateAsync())
                                      .ReturnsAsync((BlockchainState)null);

        _mockWatchedTransactionRepository.Setup(x => x.GetAllPendingAsync())
                                         .ReturnsAsync(
                                              new List<Domain.Bitcoin.Transactions.Models.WatchedTransactionModel>());

        _mockBitcoinWallet.Setup(x => x.GetCurrentBlockHeightAsync())
                          .ReturnsAsync(100u);

        _mockBitcoinWallet.Setup(x => x.GetBlockAsync(It.IsAny<uint>()))
                          .ReturnsAsync(new Block());

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _mockBlockchainStateRepository.Verify(x => x.Add(It.IsAny<BlockchainState>()), Times.Once);
    }

    [Fact]
    public async Task WatchTransactionAsync_AddsTransactionToDbAndInMemory()
    {
        // Arrange
        var channelId = new ChannelId(new byte[32]);
        var txId = new TxId(new byte[32]);
        const uint requiredDepth = 6;

        // Act
        await _service.WatchTransactionAsync(channelId, txId, requiredDepth);

        // Assert
        _mockWatchedTransactionRepository.Verify(
            x => x.Add(
                It.Is<Domain.Bitcoin.Transactions.Models.WatchedTransactionModel>(t => t.ChannelId.Equals(channelId) &&
                        t.TransactionId.Equals(txId) &&
                        t.RequiredDepth == requiredDepth)),
            Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessNewBlock_AddsMissingBlocksAndProcessesThem()
    {
        // Arrange
        var currentBlockHeight = 110u;
        var block = new Block();

        // Setup to simulate blockchain state at height 100
        var state = new BlockchainState(100, new byte[32], DateTime.UtcNow);
        _mockBlockchainStateRepository.Setup(x => x.GetStateAsync())
                                      .ReturnsAsync(state);

        _mockWatchedTransactionRepository.Setup(x => x.GetAllPendingAsync())
                                         .ReturnsAsync(
                                              new List<Domain.Bitcoin.Transactions.Models.WatchedTransactionModel>());

        _mockBitcoinWallet.Setup(x => x.GetCurrentBlockHeightAsync())
                          .ReturnsAsync(currentBlockHeight);

        _mockBitcoinWallet.Setup(x => x.GetBlockAsync(It.IsAny<uint>()))
                          .ReturnsAsync(block);

        await _service.StartAsync(CancellationToken.None);

        // Setup for tracking new block events
        var newBlockEventCalled = false;
        _service.OnNewBlockDetected += (_, _) => newBlockEventCalled = true;

        // Create a private method invoker to test ProcessNewBlock which is private
        var processNewBlockMethod = typeof(BlockchainMonitorService).GetMethod("ProcessNewBlock",
                                                                               System.Reflection.BindingFlags
                                                                                  .NonPublic |
                                                                               System.Reflection.BindingFlags.Instance);

        // Act
        await (Task)processNewBlockMethod.Invoke(_service, new object[] { block, currentBlockHeight });

        // Assert
        Assert.True(newBlockEventCalled, "New block event should have been raised");
        _mockBlockchainStateRepository.Verify(x => x.Update(It.IsAny<BlockchainState>()), Times.AtLeastOnce);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task StopAsync_CancelsTasksAndCleansUp()
    {
        // Arrange
        await _service.StartAsync(CancellationToken.None);

        // Act
        await _service.StopAsync();

        // Assert - Nothing to verify explicitly, just ensuring it doesn't throw
    }

    [Fact]
    public void OnTransactionConfirmed_RaisedWhenTransactionReachesRequiredDepth()
    {
        // Arrange
        var channelId = new ChannelId(new byte[32]);
        var txId = new TxId(new byte[32]);
        const uint requiredDepth = 1;

        var watchedTx = new Domain.Bitcoin.Transactions.Models.WatchedTransactionModel(channelId, txId, requiredDepth);
        watchedTx.SetHeightAndIndex(100, 1);

        var transactionConfirmedCalled = false;
        _service.OnTransactionConfirmed += (_, args) =>
        {
            transactionConfirmedCalled = true;
            Assert.Equal(watchedTx, args.WatchedTransaction);
        };

        // Use reflection to access private methods/fields for testing
        var checkWatchedTransactionsDepthMethod = typeof(BlockchainMonitorService).GetMethod(
            "CheckWatchedTransactionsDepth",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var watchedTransactionsField = typeof(BlockchainMonitorService).GetField("_watchedTransactions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var lastProcessedBlockHeightField = typeof(BlockchainMonitorService).GetField("_lastProcessedBlockHeight",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Set the private fields
        var watchedTransactions =
            (ConcurrentDictionary<uint256, Domain.Bitcoin.Transactions.Models.WatchedTransactionModel>)
            watchedTransactionsField.GetValue(_service);
        watchedTransactions[new uint256(txId)] = watchedTx;

        lastProcessedBlockHeightField.SetValue(_service, 101u); // Setting block height to 101 to trigger confirmation

        // Act
        checkWatchedTransactionsDepthMethod.Invoke(_service, new object[] { _mockUnitOfWork.Object });

        // Assert
        Assert.True(transactionConfirmedCalled, "Transaction confirmed event should have been raised");
        _mockWatchedTransactionRepository.Verify(
            x => x.Update(
                It.Is<Domain.Bitcoin.Transactions.Models.WatchedTransactionModel>(t => t.ChannelId.Equals(channelId) &&
                        t.IsCompleted)), Times.Once);
    }

    [Fact]
    public void CheckWatchedTransactionsForBlock_IdentifiesAndUpdatesTransactions()
    {
        // Arrange
        var channelId = new ChannelId(new byte[32]);
        var txId = new TxId(new byte[32]);
        const uint requiredDepth = 6;
        const uint blockHeight = 100;

        var watchedTx = new WatchedTransactionModel(channelId, txId, requiredDepth);

        // Create a transaction for the block
        var transaction = Transaction.Create(Network.Main);
        transaction.Inputs.Add(new OutPoint());
        var txHash = transaction.GetHash();

        var blockTransactions = new List<Transaction> { transaction };

        // Use reflection to access private methods/fields
        var checkWatchedTransactionsForBlockMethod = typeof(BlockchainMonitorService).GetMethod(
            "CheckWatchedTransactionsForBlock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var watchedTransactionsField = typeof(BlockchainMonitorService).GetField("_watchedTransactions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Set the watched transactions field
        var watchedTransactions =
            (ConcurrentDictionary<uint256, Domain.Bitcoin.Transactions.Models.WatchedTransactionModel>)
            watchedTransactionsField.GetValue(_service);
        watchedTransactions[txHash] = watchedTx;

        // Act
        checkWatchedTransactionsForBlockMethod.Invoke(
            _service, new object[] { blockTransactions, blockHeight, _mockUnitOfWork.Object });

        // Assert
        _mockWatchedTransactionRepository.Verify(
            x => x.Update(
                It.Is<WatchedTransactionModel>(t => t.ChannelId.Equals(channelId) &&
                                                    t.FirstSeenAtHeight == blockHeight)), Times.Once);

        // If the required depth is 0, it should also mark the transaction as completed
        if (watchedTx.RequiredDepth == 0)
        {
            Assert.True(watchedTx.IsCompleted);
        }
    }
}