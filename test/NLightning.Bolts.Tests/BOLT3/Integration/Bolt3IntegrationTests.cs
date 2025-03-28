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
    #region Generation Tests
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

    #region Storage Tests
    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecretsInCorrectSequence_Then_ShouldSucceed()
    {
        // Given
        using var storage = new SecretStorageService();

        // When
        var result1 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_0, AppendixDVectors.STORAGE_INDEX_MAX);
        var result2 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_1, AppendixDVectors.STORAGE_INDEX_MAX - 1);
        var result3 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_2, AppendixDVectors.STORAGE_INDEX_MAX - 2);
        var result4 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_3, AppendixDVectors.STORAGE_INDEX_MAX - 3);
        var result5 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_4, AppendixDVectors.STORAGE_INDEX_MAX - 4);
        var result6 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_5, AppendixDVectors.STORAGE_INDEX_MAX - 5);
        var result7 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_6, AppendixDVectors.STORAGE_INDEX_MAX - 6);
        var result8 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_7, AppendixDVectors.STORAGE_INDEX_MAX - 7);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.True(result6);
        Assert.True(result7);
        Assert.True(result8);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets1InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_8, AppendixDVectors.STORAGE_INDEX_MAX);

        // When
        var result2 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_1, AppendixDVectors.STORAGE_INDEX_MAX - 1);

        // Then
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets2InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_8, AppendixDVectors.STORAGE_INDEX_MAX);
        var result2 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_9, AppendixDVectors.STORAGE_INDEX_MAX - 1);
        var result3 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_2, AppendixDVectors.STORAGE_INDEX_MAX - 2);

        // When
        var result4 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_3, AppendixDVectors.STORAGE_INDEX_MAX - 3);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.False(result4);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets3InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_0, AppendixDVectors.STORAGE_INDEX_MAX);
        var result2 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_1, AppendixDVectors.STORAGE_INDEX_MAX - 1);
        var result3 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_10, AppendixDVectors.STORAGE_INDEX_MAX - 2);

        // When
        var result4 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_3, AppendixDVectors.STORAGE_INDEX_MAX - 3);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.False(result4);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets4InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_8, AppendixDVectors.STORAGE_INDEX_MAX);
        var result2 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_9, AppendixDVectors.STORAGE_INDEX_MAX - 1);
        var result3 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_10, AppendixDVectors.STORAGE_INDEX_MAX - 2);
        var result4 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_11, AppendixDVectors.STORAGE_INDEX_MAX - 3);
        var result5 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_4, AppendixDVectors.STORAGE_INDEX_MAX - 4);
        var result6 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_5, AppendixDVectors.STORAGE_INDEX_MAX - 5);
        var result7 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_6, AppendixDVectors.STORAGE_INDEX_MAX - 6);

        // When
        var result8 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_7, AppendixDVectors.STORAGE_INDEX_MAX - 7);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.True(result6);
        Assert.True(result7);
        Assert.False(result8);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets5InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_0, AppendixDVectors.STORAGE_INDEX_MAX);
        var result2 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_1, AppendixDVectors.STORAGE_INDEX_MAX - 1);
        var result3 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_2, AppendixDVectors.STORAGE_INDEX_MAX - 2);
        var result4 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_3, AppendixDVectors.STORAGE_INDEX_MAX - 3);
        var result5 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_12, AppendixDVectors.STORAGE_INDEX_MAX - 4);

        // When
        var result6 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_5, AppendixDVectors.STORAGE_INDEX_MAX - 5);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.False(result6);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets6InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_0, AppendixDVectors.STORAGE_INDEX_MAX);
        var result2 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_1, AppendixDVectors.STORAGE_INDEX_MAX - 1);
        var result3 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_2, AppendixDVectors.STORAGE_INDEX_MAX - 2);
        var result4 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_3, AppendixDVectors.STORAGE_INDEX_MAX - 3);
        var result5 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_12, AppendixDVectors.STORAGE_INDEX_MAX - 4);
        var result6 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_13, AppendixDVectors.STORAGE_INDEX_MAX - 5);
        var result7 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_6, AppendixDVectors.STORAGE_INDEX_MAX - 6);

        // When
        var result8 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_7, AppendixDVectors.STORAGE_INDEX_MAX - 7);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.True(result6);
        Assert.True(result7);
        Assert.False(result8);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets7InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_0, AppendixDVectors.STORAGE_INDEX_MAX);
        var result2 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_1, AppendixDVectors.STORAGE_INDEX_MAX - 1);
        var result3 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_2, AppendixDVectors.STORAGE_INDEX_MAX - 2);
        var result4 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_3, AppendixDVectors.STORAGE_INDEX_MAX - 3);
        var result5 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_4, AppendixDVectors.STORAGE_INDEX_MAX - 4);
        var result6 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_5, AppendixDVectors.STORAGE_INDEX_MAX - 5);
        var result7 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_14, AppendixDVectors.STORAGE_INDEX_MAX - 6);

        // When
        var result8 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_7, AppendixDVectors.STORAGE_INDEX_MAX - 7);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.True(result6);
        Assert.True(result7);
        Assert.False(result8);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_InsertingSecrets8InWrongSequence_Then_ShouldFail()
    {
        // Given
        using var storage = new SecretStorageService();

        var result1 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_0, AppendixDVectors.STORAGE_INDEX_MAX);
        var result2 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_1, AppendixDVectors.STORAGE_INDEX_MAX - 1);
        var result3 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_2, AppendixDVectors.STORAGE_INDEX_MAX - 2);
        var result4 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_3, AppendixDVectors.STORAGE_INDEX_MAX - 3);
        var result5 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_4, AppendixDVectors.STORAGE_INDEX_MAX - 4);
        var result6 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_5, AppendixDVectors.STORAGE_INDEX_MAX - 5);
        var result7 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_6, AppendixDVectors.STORAGE_INDEX_MAX - 6);

        // When
        var result8 = storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_15, AppendixDVectors.STORAGE_INDEX_MAX - 7);

        // Then
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.True(result5);
        Assert.True(result6);
        Assert.True(result7);
        Assert.False(result8);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingOldSecret_Then_ShouldDeriveCorrectly()
    {
        // Given
        using var storage = new SecretStorageService();
        Span<byte> derivedSecret = stackalloc byte[SecretStorageService.SECRET_SIZE];

        // Insert a valid secret with known index
        storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_0, AppendixDVectors.STORAGE_INDEX_MAX);
        storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_1, AppendixDVectors.STORAGE_INDEX_MAX - 1);
        storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_2, AppendixDVectors.STORAGE_INDEX_MAX - 2);

        // When
        storage.DeriveOldSecret(AppendixDVectors.STORAGE_INDEX_MAX, derivedSecret);

        // Then
        Assert.Equal(AppendixDVectors.STORAGE_EXPECTED_SECRET_0, derivedSecret);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingUnknownSecret_Then_ShouldThrowException()
    {
        // Given
        using var storage = new SecretStorageService();
        var derivedSecret = new byte[SecretStorageService.SECRET_SIZE];

        // Insert a valid secret with known index
        storage.InsertSecret(AppendixDVectors.STORAGE_EXPECTED_SECRET_1, AppendixDVectors.STORAGE_INDEX_MAX);

        // When/Then
        // Cannot derive a secret with higher index
        Assert.Throws<InvalidOperationException>(() => storage.DeriveOldSecret(AppendixDVectors.STORAGE_INDEX_MAX - 1, derivedSecret));
    }

    [Fact]
    public void Given_Bolt3Specifications_When_StoringAndDeriving48Secrets_Then_ShouldWorkCorrectly()
    {
        // Given
        using var storage = new SecretStorageService();
        Span<byte> derivedSecret = stackalloc byte[SecretStorageService.SECRET_SIZE];

        // Insert secrets with different number of trailing zeros
        for (var i = 0; i < 48; i++)
        {
            var index = (AppendixDVectors.STORAGE_INDEX_MAX - 1) & ~((1UL << i) - 1);
            var secret = KeyDerivationService.GeneratePerCommitmentSecret(
                AppendixDVectors.STORAGE_CORRECT_SEED, index);

            storage.InsertSecret(secret, index);
        }

        // When/Then
        // We should be able to derive any secret in the range
        for (var i = 1U; i < 20; i++)
        {
            var index = AppendixDVectors.STORAGE_INDEX_MAX - i;
            var expectedSecret = KeyDerivationService.GeneratePerCommitmentSecret(
                AppendixDVectors.STORAGE_CORRECT_SEED, index);

            storage.DeriveOldSecret(index, derivedSecret);

            Assert.Equal(expectedSecret, derivedSecret);
        }
    }
    #endregion
    #endregion

    #region Appendix E Vectors
    [Fact]
    public void Given_Bolt3Specifications_When_DerivingPubKey_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();
        var basepoint = new PubKey("036d6caac248af96f6afa7f904f550253a0f3ef3f5aa2fe6838a95b216691468e2");
        var perCommitmentPoint = new PubKey("025f7117a78150fe2ef97db7cfc83bd57b2e2c0d0dd25eaf467a4a1c2a45ce1486");
        var expectedLocalPubkey = new PubKey("0235f2dbfaa89b57ec7b055afe29849ef7ddfeb1cefdb9ebdc43f5494984db29e5");

        // When
        var result = keyDerivationService.DerivePublicKey(basepoint, perCommitmentPoint);

        // Then
        Assert.Equal(expectedLocalPubkey, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingPrivateKey_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();
        var baseSecretBytes = Convert.FromHexString("000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f");
        var basepointSecret = new Key(baseSecretBytes);
        var perCommitmentPoint = new PubKey("025f7117a78150fe2ef97db7cfc83bd57b2e2c0d0dd25eaf467a4a1c2a45ce1486");
        var expectedPrivkeyBytes = Convert.FromHexString("cbced912d3b21bf196a766651e436aff192362621ce317704ea2f75d87e7be0f");

        // When
        var result = keyDerivationService.DerivePrivateKey(basepointSecret, perCommitmentPoint);

        // Then
        Assert.Equal(expectedPrivkeyBytes, result.ToBytes());
    }

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingRevocationPubKey_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();
        var revocationBasepoint = new PubKey("036d6caac248af96f6afa7f904f550253a0f3ef3f5aa2fe6838a95b216691468e2");
        var perCommitmentPoint = new PubKey("025f7117a78150fe2ef97db7cfc83bd57b2e2c0d0dd25eaf467a4a1c2a45ce1486");
        var expectedRevocationPubkey = new PubKey("02916e326636d19c33f13e8c0c3a03dd157f332f3e99c317c141dd865eb01f8ff0");

        // When
        var result = keyDerivationService.DeriveRevocationPubKey(revocationBasepoint, perCommitmentPoint);

        // Then
        Assert.Equal(expectedRevocationPubkey, result);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_DerivingRevocationPrivKey_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var keyDerivationService = new KeyDerivationService();
        var baseSecretBytes = Convert.FromHexString("000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f");
        var revocationBasepointSecret = new Key(baseSecretBytes);
        var perCommitmentSecretBytes = Convert.FromHexString("1f1e1d1c1b1a191817161514131211100f0e0d0c0b0a09080706050403020100");
        var perCommitmentSecret = new Key(perCommitmentSecretBytes);
        var expectedRevocationPrivkeyBytes = Convert.FromHexString("d09ffff62ddb2297ab000cc85bcb4283fdeb6aa052affbc9dddcf33b61078110");

        // When
        var result = keyDerivationService.DeriveRevocationPrivKey(revocationBasepointSecret, perCommitmentSecret);

        // Then
        Assert.Equal(expectedRevocationPrivkeyBytes, result.ToBytes());
    }
    #endregion
}