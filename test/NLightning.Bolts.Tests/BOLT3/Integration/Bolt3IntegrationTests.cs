using NBitcoin;
using NLightning.Bolts.BOLT3.Services;

namespace NLightning.Bolts.Tests.BOLT3.Integration;

using Bolts.BOLT3.Factories;
using Bolts.BOLT3.Outputs;
using Bolts.BOLT3.Types;
using Common.Enums;
using Common.Interfaces;
using Common.Managers;
using Common.Types;
using TestCollections;
using Utils;

[Collection(ConfigManagerCollection.NAME)]
public class Bolt3IntegrationTests
{
    private readonly CommitmentNumber _commitmentNumber = new(AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                                              AppendixCVectors.NODE_B_PAYMENT_BASEPOINT,
                                                              AppendixCVectors.COMMITMENT_NUMBER);

    private readonly FundingOutput _fundingOutput = new(AppendixCVectors.NODE_A_FUNDING_PUBKEY,
                                                        AppendixCVectors.NODE_B_FUNDING_PUBKEY,
                                                        AppendixBVectors.FUNDING_SATOSHIS)
    {
        TxId = AppendixBVectors.EXPECTED_TX_ID
    };

    #region Appendix B Vectors
    [Fact]
    public void Given_Bolt3Specifications_When_CreatingFundingTransaction_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(15000, LightningMoneyUnit.SATOSHI));
        var fundingTransactionFactory = new FundingTransactionFactory(feeServiceMock.Object);

        var fundingInputCoin = new Coin(AppendixBVectors.INPUT_TX, AppendixBVectors.INPUT_INDEX);

        // When
        var fundingTransaction = fundingTransactionFactory.CreateFundingTransaction(AppendixBVectors.LOCAL_PUB_KEY,
                                                                                    AppendixBVectors.REMOTE_PUB_KEY,
                                                                                    AppendixBVectors.FUNDING_SATOSHIS,
                                                                                    AppendixBVectors.CHANGE_SCRIPT.PaymentScript,
                                                                                    AppendixBVectors.CHANGE_SCRIPT,
                                                                                    [fundingInputCoin],
                                                                                    new BitcoinSecret(
                                                                                        AppendixBVectors.INPUT_SIGNING_PRIV_KEY,
                                                                                        ConfigManager.Instance.Network));
        var finalFundingTx = fundingTransaction.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixBVectors.EXPECTED_TX.ToBytes(), finalFundingTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }
    #endregion

    #region Appendix C Vectors
    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransaction_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(15000, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(_fundingOutput.ToCoin(),
                                                                                            AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                                                                            AppendixCVectors.NODE_B_PAYMENT_BASEPOINT,
                                                                                            AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                                                                            AppendixCVectors.NODE_A_REVOCATION_PUBKEY,
                                                                                            AppendixCVectors.TX0_TO_LOCAL_MSAT,
                                                                                            AppendixCVectors.TO_REMOTE_MSAT,
                                                                                            AppendixCVectors.LOCAL_DELAY,
                                                                                            _commitmentNumber, true,
                                                                                            new BitcoinSecret(
                                                                                                AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                                                                ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_0, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_0.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith5HTLCsUntrimmed_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(LightningMoney.Zero);
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<OfferedHtlcOutput> offeredHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC2_PAYMENT_HASH,
                AppendixCVectors.HTLC2_AMOUNT, AppendixCVectors.HTLC2_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC3_PAYMENT_HASH,
                AppendixCVectors.HTLC3_AMOUNT, AppendixCVectors.HTLC3_CLTV_EXPIRY),
        ];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC0_PAYMENT_HASH,
                AppendixCVectors.HTLC0_AMOUNT, AppendixCVectors.HTLC0_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC1_PAYMENT_HASH,
                AppendixCVectors.HTLC1_AMOUNT, AppendixCVectors.HTLC1_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_1, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_1.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith7OutputsUntrimmed_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(647, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<OfferedHtlcOutput> offeredHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC2_PAYMENT_HASH,
                AppendixCVectors.HTLC2_AMOUNT, AppendixCVectors.HTLC2_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC3_PAYMENT_HASH,
                AppendixCVectors.HTLC3_AMOUNT, AppendixCVectors.HTLC3_CLTV_EXPIRY),
        ];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC0_PAYMENT_HASH,
                AppendixCVectors.HTLC0_AMOUNT, AppendixCVectors.HTLC0_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC1_PAYMENT_HASH,
                AppendixCVectors.HTLC1_AMOUNT, AppendixCVectors.HTLC1_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_2, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_2.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith6OutputsUntrimmed_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(648, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<OfferedHtlcOutput> offeredHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC2_PAYMENT_HASH,
                AppendixCVectors.HTLC2_AMOUNT, AppendixCVectors.HTLC2_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC3_PAYMENT_HASH,
                AppendixCVectors.HTLC3_AMOUNT, AppendixCVectors.HTLC3_CLTV_EXPIRY),
        ];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC1_PAYMENT_HASH,
                AppendixCVectors.HTLC1_AMOUNT, AppendixCVectors.HTLC1_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_3, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_3.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith6OutputsUntrimmedMaxFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(2069, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<OfferedHtlcOutput> offeredHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC2_PAYMENT_HASH,
                AppendixCVectors.HTLC2_AMOUNT, AppendixCVectors.HTLC2_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC3_PAYMENT_HASH,
                AppendixCVectors.HTLC3_AMOUNT, AppendixCVectors.HTLC3_CLTV_EXPIRY),
        ];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC1_PAYMENT_HASH,
                AppendixCVectors.HTLC1_AMOUNT, AppendixCVectors.HTLC1_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_4, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_4.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith5OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(2070, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<OfferedHtlcOutput> offeredHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                  AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC2_PAYMENT_HASH,
                                  AppendixCVectors.HTLC2_AMOUNT, AppendixCVectors.HTLC2_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                  AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC3_PAYMENT_HASH,
                                  AppendixCVectors.HTLC3_AMOUNT, AppendixCVectors.HTLC3_CLTV_EXPIRY),
        ];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                   AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                                   AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_5, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_5.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith5OutputsUntrimmedMaxFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(2194, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<OfferedHtlcOutput> offeredHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                  AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC2_PAYMENT_HASH,
                                  AppendixCVectors.HTLC2_AMOUNT, AppendixCVectors.HTLC2_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                  AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC3_PAYMENT_HASH,
                                  AppendixCVectors.HTLC3_AMOUNT, AppendixCVectors.HTLC3_CLTV_EXPIRY),
        ];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                   AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                                   AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_6, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_6.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith4OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(2195, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<OfferedHtlcOutput> offeredHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                  AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC3_PAYMENT_HASH,
                                  AppendixCVectors.HTLC3_AMOUNT, AppendixCVectors.HTLC3_CLTV_EXPIRY),
        ];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                   AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                                   AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_7, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_7.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith4OutputsUntrimmedMaxFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(3702, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<OfferedHtlcOutput> offeredHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                  AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC3_PAYMENT_HASH,
                                  AppendixCVectors.HTLC3_AMOUNT, AppendixCVectors.HTLC3_CLTV_EXPIRY),
        ];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                   AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                                   AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_8, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_8.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith3OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(3703, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<OfferedHtlcOutput> offeredHtlcs = [];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                   AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                                   AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_9, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_9.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith3OutputsUntrimmedMaxFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(4914, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                   AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                                   AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, [], receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_10, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_10.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith2OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(4915, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_11, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_11.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith2OutputsUntrimmedMaxFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(9651180, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_12, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_12.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith1OutputsUntrimmedMinFeeRate_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(9651181, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_13, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_13.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWithFeeGreaterThanFunderAmount_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(9651936, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_14, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_14.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransactionWith2SimilarOfferedHtlc_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(253, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        List<OfferedHtlcOutput> offeredHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC5_PAYMENT_HASH,
                AppendixCVectors.HTLC5_AMOUNT, AppendixCVectors.HTLC5_CLTV_EXPIRY),
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC6_PAYMENT_HASH,
                AppendixCVectors.HTLC6_AMOUNT, AppendixCVectors.HTLC6_CLTV_EXPIRY),
        ];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC1_PAYMENT_HASH,
                AppendixCVectors.HTLC1_AMOUNT, AppendixCVectors.HTLC1_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(_fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX15_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, _commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs, new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE_15, _fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_15.ToBytes(), finalCommitmentTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }
    #endregion

    #region Appendix D Vectors
    [Fact]
    public void Given_Bolt3Specifications_When_GeneratingFromSeed0FinalNode_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();

        // When
        var result = KeyDerivationService.GeneratePerCommitmentSecret(AppendixDVectors.SEED_0_FINAL_NODE,
                                                                      AppendixDVectors.I_0_FINAL_NODE);

        // Then
        Assert.Equal(AppendixDVectors.EXPECTED_OUTPUT_0_FINAL_NODE, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_GeneratingFromSeedFFFinalNode_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();

        // When
        var result = KeyDerivationService.GeneratePerCommitmentSecret(AppendixDVectors.SEED_FF_FINAL_NODE,
                                                                      AppendixDVectors.I_FF_FINAL_NODE);

        // Then
        Assert.Equal(AppendixDVectors.EXPECTED_OUTPUT_FF_FINAL_NODE, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_GeneratingFromSeedFFAlternateBits1_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();

        // When
        var result = KeyDerivationService.GeneratePerCommitmentSecret(AppendixDVectors.SEED_FF_ALTERNATE_BITS_1,
                                                                      AppendixDVectors.I_FF_ALTERNATE_BITS_1);

        // Then
        Assert.Equal(AppendixDVectors.EXPECTED_OUTPUT_FF_ALTERNATE_BITS_1, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_GeneratingFromSeedFFAlternateBits2_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();

        // When
        var result = KeyDerivationService.GeneratePerCommitmentSecret(AppendixDVectors.SEED_FF_ALTERNATE_BITS_2,
                                                                      AppendixDVectors.I_FF_ALTERNATE_BITS_2);

        // Then
        Assert.Equal(AppendixDVectors.EXPECTED_OUTPUT_FF_ALTERNATE_BITS_2, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_GeneratingFromSeed01LastNonTrivialNode_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();

        // When
        var result = KeyDerivationService.GeneratePerCommitmentSecret(AppendixDVectors.SEED_01_LAST_NON_TRIVIAL_NODE,
                                                                      AppendixDVectors.I_01_LAST_NON_TRIVIAL_NODE);

        // Then
        Assert.Equal(AppendixDVectors.EXPECTED_OUTPUT_01_LAST_NON_TRIVIAL_NODE, result);
    }
    #endregion
}