using NBitcoin;

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
        var fundingTransaction = fundingTransactionFactory.CreateFundingTransaction(
            AppendixBVectors.LOCAL_PUB_KEY,
            AppendixBVectors.REMOTE_PUB_KEY,
            AppendixBVectors.FUNDING_SATOSHIS,
            AppendixBVectors.CHANGE_SCRIPT.PaymentScript,
            AppendixBVectors.CHANGE_SCRIPT,
            [fundingInputCoin],
            new BitcoinSecret(AppendixBVectors.INPUT_SIGNING_PRIV_KEY, ConfigManager.Instance.Network));
        var finalFundingTx = fundingTransaction.GetSignedTransaction();

        // Then
        // 0200000001ADBB20EA41A8423EA937E76E8151636BF6093B70EAFF942930D20576600521FD000000006B483045022100 90587B6201E166AD6AF0227D3036A9454223D49A1F11839C1A362184340EF0240220577F7CD5CCA78719405CBF1DE7414AC027F0239EF6E214C90FCAAB0454D84B3B012103535B32D5EB0A6ED0982A0479BBADC9868D9836F6BA94DD5A63BE16D875069184FFFFFFFF028096980000000000220020C015C4A6BE010E21657068FC2E6A9D02B27EBE4D490A25846F7237F104D1A3CD20256D29010000001600143CA33C2E4446F4A305F23C80DF8AD1AFDCF652F900000000
        // 0200000001ADBB20EA41A8423EA937E76E8151636BF6093B70EAFF942930D20576600521FD000000006B483045022100 BE957CD93351C0DA77CB8B4A64C1E34E3D4321FF5ED338A37F3087C8661C59A002206A728F7AE44C8BEC98FB4634AF46F116EC9047483A082E52941E9587AE5A4259012103535B32D5EB0A6ED0982A0479BBADC9868D9836F6BA94DD5A63BE16D875069184FFFFFFFF0000000000
        Assert.Equal(AppendixBVectors.EXPECTED_TX.ToBytes(), finalFundingTx.ToBytes());

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransaction_Then_ShouldBeEqualToTestVector()
    {
        // Given
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(new LightningMoney(15000, LightningMoneyUnit.SATOSHI));
        var commitmentTransactionFactory = new CommitmentTransactionFactory(feeServiceMock.Object);

        var commitmentNumber = new CommitmentNumber(AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
            AppendixCVectors.NODE_B_PAYMENT_BASEPOINT,
            AppendixCVectors.COMMITMENT_NUMBER);

        var fundingOutput = new FundingOutput(AppendixCVectors.NODE_A_FUNDING_PUBKEY,
                                              AppendixCVectors.NODE_B_FUNDING_PUBKEY,
                                              AppendixBVectors.FUNDING_SATOSHIS)
        {
            TxId = AppendixBVectors.EXPECTED_TX_ID
        };

        // When
        var commitmentTransacion = commitmentTransactionFactory.CreateCommitmentTransaction(fundingOutput.ToCoin(),
            AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
            AppendixCVectors.NODE_B_PAYMENT_BASEPOINT,
            AppendixCVectors.NODE_A_DELAYED_PUBKEY,
            AppendixCVectors.NODE_A_REVOCATION_PUBKEY,
            AppendixCVectors.TX0_TO_LOCAL_MSAT,
            AppendixCVectors.TO_REMOTE_MSAT,
            AppendixCVectors.LOCAL_DELAY,
            commitmentNumber, true,
            new BitcoinSecret(
                AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE, fundingOutput.RemotePubKey);

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

        var commitmentNumber = new CommitmentNumber(AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
            AppendixCVectors.NODE_B_PAYMENT_BASEPOINT,
            AppendixCVectors.COMMITMENT_NUMBER);

        var fundingOutput = new FundingOutput(AppendixCVectors.NODE_A_FUNDING_PUBKEY,
            AppendixCVectors.NODE_B_FUNDING_PUBKEY,
            AppendixBVectors.FUNDING_SATOSHIS)
        {
            TxId = AppendixBVectors.EXPECTED_TX_ID
        };

        List<OfferedHtlcOutput> offeredHtlcs = [
            new OfferedHtlcOutput(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                  AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC2_PAYMENT_HASH,
                                  AppendixCVectors.HTLC2_AMOUNT, AppendixCVectors.HTLC2_CLTV_EXPIRY),
            new OfferedHtlcOutput(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                  AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC3_PAYMENT_HASH,
                                  AppendixCVectors.HTLC3_AMOUNT, AppendixCVectors.HTLC3_CLTV_EXPIRY),
        ];

        List<ReceivedHtlcOutput> receivedHtlcs = [
            new ReceivedHtlcOutput(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                   AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC0_PAYMENT_HASH,
                                   AppendixCVectors.HTLC0_AMOUNT, AppendixCVectors.HTLC0_CLTV_EXPIRY),
            new ReceivedHtlcOutput(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                   AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC1_PAYMENT_HASH,
                                   AppendixCVectors.HTLC1_AMOUNT, AppendixCVectors.HTLC1_CLTV_EXPIRY),
            new ReceivedHtlcOutput(AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.NODE_B_HTLC_PUBKEY,
                                   AppendixCVectors.NODE_A_HTLC_PUBKEY, AppendixCVectors.HTLC4_PAYMENT_HASH,
                                   AppendixCVectors.HTLC4_AMOUNT, AppendixCVectors.HTLC4_CLTV_EXPIRY),
        ];

        // When
        var commitmentTransacion = commitmentTransactionFactory.
            CreateCommitmentTransaction(fundingOutput.ToCoin(), AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                        AppendixCVectors.NODE_B_PAYMENT_BASEPOINT, AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                        AppendixCVectors.NODE_A_REVOCATION_PUBKEY, AppendixCVectors.TX1_TO_LOCAL_MSAT,
                                        AppendixCVectors.TO_REMOTE_MSAT, AppendixCVectors.LOCAL_DELAY, commitmentNumber,
                                        true, offeredHtlcs, receivedHtlcs,
                                        new BitcoinSecret(AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                          ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE, fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(LightningMoney.FromUnit(6988000, LightningMoneyUnit.SATOSHI), commitmentTransacion.ToLocalOutput.Amount);
        Assert.Equal(LightningMoney.FromUnit(3000000, LightningMoneyUnit.SATOSHI), commitmentTransacion.ToRemoteOutput.Amount);
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_1.GetHash(), commitmentTransacion.TxId);

        ConfigManagerUtil.ResetConfigManager();
    }

    // Simulate adding HTLCs
    // - Add HTLCs to the channel.
    // - Ensure correct handling of trimmed HTLCs.

    // Validate the commitment transaction with HTLCs for Node A
    // - Ensure the transaction includes the correct HTLC outputs.

    // Simulate HTLC-timeout and HTLC-success transactions
    // - Generate HTLC-timeout and HTLC-success transactions.

    // Validate HTLC-timeout transaction
    // - Ensure the transaction adheres to BOLT 3 specifications.

    // Validate HTLC-success transaction
    // - Ensure the transaction adheres to BOLT 3 specifications.

    // Simulate closing the channel
    // - Generate the closing transaction.

    // Validate the closing transaction
    // - Ensure the transaction adheres to BOLT 3 specifications.

    // Ensure outputs are ordered correctly (BIP 69+CLTV)
    // - Check the output ordering.
}