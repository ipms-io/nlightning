using NBitcoin;
using NLightning.Bolts.Tests.TestCollections;
using NLightning.Bolts.Tests.Utils;

namespace NLightning.Bolts.Tests.BOLT3.Transactions;

using Bolts.BOLT3.Transactions;
using Common.Enums;
using Common.Interfaces;
using Common.Managers;
using Common.Types;

[Collection(ConfigManagerCollection.NAME)]
public class FundingTransactionTests
{
    private readonly PubKey _localPubKey = new("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");
    private readonly PubKey _remotePubKey = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    private readonly LightningMoney _fundingAmount = LightningMoney.FromUnit(1_000, LightningMoneyUnit.SATOSHI);
    private readonly Script _changeScript = Script.FromHex("002032E8DA66B7054D40832C6A7A66DF79D8D7BCCCD5FFA53F5DD1772CB9CB9F3283");
    private readonly Script _redeemScript = Script.FromHex("21034F355BDCB7CC0AF728EF3CCEB9615D90684BB5B2CA5F859AB0F0B704075871AAAD51B2");
    private readonly Coin[] _coins =
    [
        new Coin(new OutPoint(uint256.Parse("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"), 0),
                 new TxOut(Money.Satoshis(2_000), Script.FromHex("0014c5ac364661c2f1e5a7a3b1bb1b8bbbc7cd89bff3")))
    ];
    private readonly BitcoinSecret _privateKey = new(new Key(Convert.FromHexString("6bd078650fcee8444e4e09825227b801a1ca928debb750eb36e6d56124bb20e8")), NBitcoin.Network.TestNet);

    [Fact]
    public void Given_ValidParameters_When_ConstructingFundingTransaction_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var fundingTransaction = new FundingTransaction(_localPubKey, _remotePubKey, _fundingAmount, _changeScript, _coins);

        // Then
        Assert.NotNull(fundingTransaction.FundingOutput);
        Assert.NotNull(fundingTransaction.ChangeOutput);
        Assert.Equal(_fundingAmount, fundingTransaction.FundingOutput.Amount);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_ValidParametersWithRedeemScript_When_ConstructingFundingTransaction_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var fundingTx = new FundingTransaction(_localPubKey, _remotePubKey, _fundingAmount, _redeemScript, _changeScript, _coins);

        // Then
        Assert.NotNull(fundingTx.FundingOutput);
        Assert.NotNull(fundingTx.ChangeOutput);
        Assert.Equal(_fundingAmount, fundingTx.FundingOutput.Amount);
        Assert.Equal(_redeemScript, fundingTx.ChangeOutput.RedeemScript);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_NullPubKey1_When_ConstructingFundingTransaction_Then_ThrowsArgumentNullException()
    {
        // Given
        PubKey pubKey1 = null;

        // When/Then
        Assert.Throws<ArgumentNullException>(() => new FundingTransaction(pubKey1, _remotePubKey, _fundingAmount, _changeScript, _coins));

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_NullPubKey2_When_ConstructingFundingTransaction_Then_ThrowsArgumentNullException()
    {
        // Given
        PubKey pubKey2 = null;

        // When/Then
        Assert.Throws<ArgumentNullException>(() => new FundingTransaction(_localPubKey, pubKey2, _fundingAmount, _changeScript, _coins));

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_IdenticalPubKeys_When_ConstructingFundingTransaction_Then_ThrowsArgumentException()
    {
        // Given
        var pubKey2 = _localPubKey; // Same as pubKey1

        // When/Then
        var exception = Assert.Throws<ArgumentException>(() => new FundingTransaction(_localPubKey, pubKey2, _fundingAmount, _changeScript, _coins));
        Assert.Contains("Public keys must be different", exception.Message);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_ZeroAmount_When_ConstructingFundingTransaction_Then_ThrowsArgumentException()
    {
        // Given
        var amount = LightningMoney.Zero;

        // When/Then
        var exception = Assert.Throws<ArgumentException>(() =>
            new FundingTransaction(_localPubKey, _remotePubKey, amount, _changeScript, _coins));
        Assert.Contains("Funding amount must be greater than zero", exception.Message);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_UnsignedTransaction_When_GetSignedTransactionCalled_Then_ThrowsInvalidOperationException()
    {
        // Given
        var fundingTx = new FundingTransaction(_localPubKey, _remotePubKey, _fundingAmount, _changeScript, _coins);

        // When/Then
        Assert.Throws<InvalidOperationException>(() => fundingTx.GetSignedTransaction());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_ValidTransaction_When_SignTransaction_Then_OutputsHaveCorrectProperties()
    {
        // Given
        var mockFeeService = new Mock<IFeeService>();
        mockFeeService.Setup(c => c.GetCachedFeeRatePerKw()).Returns(LightningMoney.FromUnit(10, LightningMoneyUnit.SATOSHI));

        ConfigManager.Instance.DustLimitAmount = LightningMoney.FromUnit(546, LightningMoneyUnit.SATOSHI);

        var fundingTx = new FundingTransaction(_localPubKey, _remotePubKey, _fundingAmount, _changeScript, _coins);

        // When
        fundingTx.SignTransaction(mockFeeService.Object, _privateKey);

        // Then
        Assert.NotNull(fundingTx.FundingOutput.TxId);
        Assert.NotNull(fundingTx.ChangeOutput.TxId);
        Assert.Equal(fundingTx.TxId, fundingTx.FundingOutput.TxId);
        Assert.Equal(fundingTx.TxId, fundingTx.ChangeOutput.TxId);

        // The change amount should be: input (200000) - funding (100000) - fee (500) = 995
        Assert.Equal(LightningMoney.FromUnit(995, LightningMoneyUnit.SATOSHI), fundingTx.ChangeOutput.Amount);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_ValidTransactionButNoChange_When_SignTransaction_Then_OnlyFundingOutputHasTxId()
    {
        // Given
        var mockFeeService = new Mock<IFeeService>();
        mockFeeService.Setup(c => c.GetCachedFeeRatePerKw()).Returns(LightningMoney.FromUnit(300, LightningMoneyUnit.SATOSHI));

        ConfigManager.Instance.DustLimitAmount = LightningMoney.FromUnit(900, LightningMoneyUnit.SATOSHI);

        var fundingTx = new FundingTransaction(_localPubKey, _remotePubKey, _fundingAmount, _changeScript, _coins);

        // When
        fundingTx.SignTransaction(mockFeeService.Object, _privateKey);

        // Then
        Assert.NotNull(fundingTx.FundingOutput.TxId);
        Assert.Equal(fundingTx.TxId, fundingTx.FundingOutput.TxId);
        Assert.Equal(0U, fundingTx.FundingOutput.Index);
        Assert.Equal(LightningMoney.Zero, fundingTx.ChangeOutput.Amount);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_SignedTransaction_When_GetSignedTransactionCalled_Then_ReturnsFinalizedTransaction()
    {
        // Given
        var mockFeeService = new Mock<IFeeService>();
        mockFeeService.Setup(c => c.GetCachedFeeRatePerKw()).Returns(LightningMoney.FromUnit(500, LightningMoneyUnit.SATOSHI));

        var fundingTx = new FundingTransaction(_localPubKey, _remotePubKey, _fundingAmount, _changeScript, _coins);
        fundingTx.SignTransaction(mockFeeService.Object, _privateKey);

        // When
        var signedTx = fundingTx.GetSignedTransaction();

        // Then
        Assert.NotNull(signedTx);
        Assert.Equal(fundingTx.TxId, signedTx.GetHash());

        ConfigManagerUtil.ResetConfigManager();
    }
}