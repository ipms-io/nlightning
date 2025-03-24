using NBitcoin;
using NBitcoin.Crypto;
using NLightning.Bolts.Tests.TestCollections;
using NLightning.Bolts.Tests.Utils;

namespace NLightning.Bolts.Tests.BOLT3.Transactions;

using Bolts.BOLT3.Outputs;
using Bolts.BOLT3.Transactions;
using Bolts.BOLT3.Types;
using Common.Enums;
using Common.Interfaces;
using Common.Managers;
using Common.Types;

[Collection(ConfigManagerCollection.NAME)]
public class CommitmentTransactionTests
{
    private const uint TO_SELF_DELAY = 144;
    private readonly PubKey _localPaymentBasepoint = new("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");
    private readonly PubKey _remotePaymentBasepoint = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    private readonly PubKey _localDelayedPubKey = new("03fd5960528dc152014952efdb702a88f71e3c1653b2314431701ec77e57fde83c");
    private readonly PubKey _revocationPubKey = new("0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19");
    private readonly Coin _fundingCoin;
    private readonly LightningMoney _toLocalAmount = LightningMoney.FromUnit(8_000, LightningMoneyUnit.SATOSHI);
    private readonly LightningMoney _toRemoteAmount = LightningMoney.FromUnit(2_000, LightningMoneyUnit.SATOSHI);
    private readonly CommitmentNumber _commitmentNumber;
    private readonly BitcoinSecret _privateKey = new(new Key(Convert.FromHexString("6bd078650fcee8444e4e09825227b801a1ca928debb750eb36e6d56124bb20e8")), NBitcoin.Network.TestNet);

    public CommitmentTransactionTests()
    {
        _fundingCoin = new Coin(new OutPoint(uint256.Parse("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"), 0),
                                new TxOut(Money.Satoshis(1_000_000), Script.FromHex("00204d71a6613871a56c112436fe6b6850d53c5c8e57")));

        _commitmentNumber = new CommitmentNumber(_localPaymentBasepoint, _remotePaymentBasepoint, 42);
    }

    [Fact]
    public void Given_ValidParametersAsChannelFunder_When_ConstructingCommitmentTransaction_Then_PropertiesAreSetCorrectly()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;
        const bool IS_CHANNEL_FUNDER = true;

        // When
        var commitmentTx = CreateCommitmentTransaction(IS_CHANNEL_FUNDER);

        // Then
        Assert.Equal(_commitmentNumber, commitmentTx.CommitmentNumber);

        Assert.NotNull(commitmentTx.ToLocalOutput);
        Assert.NotNull(commitmentTx.ToRemoteOutput);
        Assert.Null(commitmentTx.LocalAnchorOutput);
        Assert.Null(commitmentTx.RemoteAnchorOutput);

        Assert.Equal(LightningMoney.Zero, commitmentTx.ToLocalOutput.Amount);
        Assert.Equal(_toRemoteAmount, commitmentTx.ToRemoteOutput.Amount);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_ValidParametersAsNonFunder_When_ConstructingCommitmentTransaction_Then_PropertiesAreSetCorrectly()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;
        const bool IS_CHANNEL_FUNDER = false;

        // When
        var commitmentTx = CreateCommitmentTransaction(IS_CHANNEL_FUNDER);

        // Then
        Assert.Equal(_commitmentNumber, commitmentTx.CommitmentNumber);

        Assert.NotNull(commitmentTx.ToLocalOutput);
        Assert.NotNull(commitmentTx.ToRemoteOutput);
        Assert.Null(commitmentTx.LocalAnchorOutput);
        Assert.Null(commitmentTx.RemoteAnchorOutput);

        Assert.Equal(_toLocalAmount, commitmentTx.ToLocalOutput.Amount);
        Assert.Equal(LightningMoney.Zero, commitmentTx.ToRemoteOutput.Amount);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_AnchorOutputsEnabled_When_ConstructingCommitmentTransaction_Then_CreatesAnchorOutputs()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = true;
        ConfigManager.Instance.AnchorAmount = LightningMoney.FromUnit(330, LightningMoneyUnit.SATOSHI);
        const bool IS_CHANNEL_FUNDER = true;

        // When
        var commitmentTx = CreateCommitmentTransaction(IS_CHANNEL_FUNDER);

        // Then
        Assert.NotNull(commitmentTx.LocalAnchorOutput);
        Assert.NotNull(commitmentTx.RemoteAnchorOutput);
        Assert.Equal(ConfigManager.Instance.AnchorAmount, commitmentTx.LocalAnchorOutput.Amount);
        Assert.Equal(ConfigManager.Instance.AnchorAmount, commitmentTx.RemoteAnchorOutput.Amount);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_NullLocalBasepoint_When_ConstructingCommitmentTransaction_Then_ThrowsArgumentNullException()
    {
        // Given

        // When/Then
        Assert.Throws<ArgumentNullException>(() => new CommitmentTransaction(_fundingCoin, null, _remotePaymentBasepoint,
                                                                             _localDelayedPubKey, _revocationPubKey,
                                                                             _toLocalAmount, _toRemoteAmount,
                                                                             TO_SELF_DELAY, _commitmentNumber, true));

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_NullRemoteBasepoint_When_ConstructingCommitmentTransaction_Then_ThrowsArgumentNullException()
    {
        // Given

        // When/Then
        Assert.Throws<ArgumentNullException>(() => new CommitmentTransaction(_fundingCoin, _localPaymentBasepoint, null,
                                                                             _localDelayedPubKey, _revocationPubKey,
                                                                             _toLocalAmount, _toRemoteAmount,
                                                                             TO_SELF_DELAY, _commitmentNumber, true));

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_NullLocalDelayedKey_When_ConstructingCommitmentTransaction_Then_ThrowsArgumentNullException()
    {
        // Given

        // When/Then
        Assert.Throws<ArgumentNullException>(() => new CommitmentTransaction(_fundingCoin, _localPaymentBasepoint,
                                                                             _remotePaymentBasepoint, null,
                                                                             _revocationPubKey, _toLocalAmount,
                                                                             _toRemoteAmount, TO_SELF_DELAY,
                                                                             _commitmentNumber, true));

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_NullRevocationKey_When_ConstructingCommitmentTransaction_Then_ThrowsArgumentNullException()
    {
        // Given

        // When/Then
        Assert.Throws<ArgumentNullException>(() => new CommitmentTransaction(_fundingCoin, _localPaymentBasepoint,
                                                                             _remotePaymentBasepoint, _localDelayedPubKey,
                                                                             null, _toLocalAmount, _toRemoteAmount,
                                                                             TO_SELF_DELAY, _commitmentNumber, true));

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_ZeroAmounts_When_ConstructingCommitmentTransaction_Then_ThrowsArgumentException()
    {
        // Given

        // When/Then
        var exception = Assert.Throws<ArgumentException>(() => new CommitmentTransaction(_fundingCoin,
                                                                                         _localPaymentBasepoint,
                                                                                         _remotePaymentBasepoint,
                                                                                         _localDelayedPubKey,
                                                                                         _revocationPubKey,
                                                                                         LightningMoney.Zero,
                                                                                         LightningMoney.Zero,
                                                                                         TO_SELF_DELAY, _commitmentNumber,
                                                                                         true));

        Assert.Contains("Both toLocalAmount and toRemoteAmount cannot be zero", exception.Message);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_AmountBelowDustLimitForFunder_When_SigningTransaction_Then_RemovesToLocalOutput()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;
        ConfigManager.Instance.DustLimitAmount = LightningMoney.FromUnit(1_999, LightningMoneyUnit.SATOSHI);
        const bool IS_CHANNEL_FUNDER = true;
        var commitmentTx = CreateCommitmentTransaction(IS_CHANNEL_FUNDER);
        var mockFeeService = new Mock<IFeeService>();
        mockFeeService.Setup(s => s.GetCachedFeeRatePerKw()).Returns(LightningMoney.FromUnit(15_000, LightningMoneyUnit.SATOSHI));

        // When
        commitmentTx.SignTransaction(mockFeeService.Object, _privateKey);
        var signedTx = commitmentTx.GetSignedTransaction();

        // Then
        Assert.Single(signedTx.Outputs); // Only toRemote output remains
        Assert.Equal(LightningMoney.Zero, commitmentTx.ToLocalOutput.Amount); // ToLocal output was removed
        Assert.Equal(commitmentTx.TxId, commitmentTx.ToRemoteOutput.TxId);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_AmountBelowDustLimitForNonFunder_When_SigningTransaction_Then_RemovesToRemoteOutput()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;
        ConfigManager.Instance.DustLimitAmount = LightningMoney.FromUnit(600, LightningMoneyUnit.SATOSHI);
        const bool IS_CHANNEL_FUNDER = false;
        var commitmentTx = CreateCommitmentTransaction(IS_CHANNEL_FUNDER);
        var mockFeeService = new Mock<IFeeService>();
        mockFeeService.Setup(s => s.GetCachedFeeRatePerKw()).Returns(LightningMoney.FromUnit(15_000, LightningMoneyUnit.SATOSHI));

        // When
        commitmentTx.SignTransaction(mockFeeService.Object, _privateKey);
        var signedTx = commitmentTx.GetSignedTransaction();

        // Then
        Assert.Single(signedTx.Outputs); // Only toLocal output remains
        Assert.Equal(LightningMoney.Zero, commitmentTx.ToRemoteOutput.Amount); // ToRemote output was removed
        Assert.Equal(commitmentTx.TxId, commitmentTx.ToLocalOutput.TxId);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_AddedHtlcOutputs_When_SigningTransaction_Then_OutputsHaveCorrectProperties()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;
        const bool IS_CHANNEL_FUNDER = true;
        var commitmentTx = CreateCommitmentTransaction(IS_CHANNEL_FUNDER);

        var htlcOffered = new OfferedHtlcOutput(_localPaymentBasepoint, _revocationPubKey, _localPaymentBasepoint,
                                                new ReadOnlyMemory<byte>([0]), new LightningMoney(50000), 500);
        var htlcReceived = new ReceivedHtlcOutput(_localPaymentBasepoint, _revocationPubKey, _localPaymentBasepoint,
                                                  new ReadOnlyMemory<byte>([0]), new LightningMoney(40000), 500);

        commitmentTx.AddOfferedHtlcOutputAndUpdate(htlcOffered);
        commitmentTx.AddReceivedHtlcOutputAndUpdate(htlcReceived);

        var mockFeeService = new Mock<IFeeService>();
        mockFeeService.Setup(s => s.GetCachedFeeRatePerKw()).Returns(new LightningMoney(100000));

        // When
        commitmentTx.SignTransaction(mockFeeService.Object, _privateKey);
        var signedTx = commitmentTx.GetSignedTransaction();

        // Then
        Assert.Equal(4, signedTx.Outputs.Count); // to_local, to_remote, offered_htlc, received_htlc
        Assert.Equal(commitmentTx.TxId, htlcOffered.TxId);
        Assert.Equal(commitmentTx.TxId, htlcReceived.TxId);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_UnsignedTransaction_When_GetSignedTransactionCalled_Then_ThrowsInvalidOperationException()
    {
        // Given
        var commitmentTx = CreateCommitmentTransaction(true);

        // When/Then
        Assert.Throws<InvalidOperationException>(() => commitmentTx.GetSignedTransaction());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_SignedTransaction_When_GetSignedTransactionCalled_Then_ReturnsFinalizedTransaction()
    {
        // Given
        var commitmentTx = CreateCommitmentTransaction(true);
        var mockFeeService = new Mock<IFeeService>();
        mockFeeService.Setup(s => s.GetCachedFeeRatePerKw()).Returns(LightningMoney.FromUnit(1_000, LightningMoneyUnit.SATOSHI));
        commitmentTx.SignTransaction(mockFeeService.Object, _privateKey);

        // When
        var signedTx = commitmentTx.GetSignedTransaction();

        // Then
        Assert.NotNull(signedTx);
        Assert.Equal(commitmentTx.TxId, signedTx.GetHash());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_RemoteSignature_When_AppendRemoteSignatureAndSign_Then_SignsTransaction()
    {
        // Given
        var commitmentTx = CreateCommitmentTransaction(true);
        var remoteSignature = new ECDSASignature(Convert.FromHexString("3045022100c3127b33dcc741dd6b05b1e63cbd1a9a7d816f37af9b6756fa2376b056f032370220408b96279808fe57eb7e463710804cdf4f108388bc5cf722d8c848d2c7f9f3b0"));

        // When
        commitmentTx.AppendRemoteSignatureAndSign(remoteSignature, _remotePaymentBasepoint);
        var transaction = commitmentTx.GetSignedTransaction();

        // Then
        Assert.NotNull(transaction);

        ConfigManagerUtil.ResetConfigManager();
    }

    private CommitmentTransaction CreateCommitmentTransaction(bool isChannelFunder)
    {
        return new CommitmentTransaction(_fundingCoin, _localPaymentBasepoint, _remotePaymentBasepoint,
                                         _localDelayedPubKey, _revocationPubKey, _toLocalAmount, _toRemoteAmount,
                                         TO_SELF_DELAY, _commitmentNumber, isChannelFunder);
    }
}