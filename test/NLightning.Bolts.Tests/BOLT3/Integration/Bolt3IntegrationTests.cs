using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Integration;

using Bolts.BOLT3.Factories;
using Bolts.BOLT3.Outputs;
using Bolts.BOLT3.Types;
using Common.Enums;
using Common.Interfaces;
using Common.Managers;
using Common.Types;

public class Bolt3IntegrationTests : IDisposable
{
    private readonly FundingTransactionFactory _fundingTransactionFactory;
    private readonly CommitmentTransactionFactory _commitmentTransactionFactory;
    private readonly bool _previousIsOptionAnchorOutput = ConfigManager.Instance.IsOptionAnchorOutput;

    public Bolt3IntegrationTests()
    {
        // Set Anchor = OFF
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeRatePerKw = new LightningMoney(15000, LightningMoneyUnit.SATOSHI); // From test vector
        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(feeRatePerKw);

        var feeService = feeServiceMock.Object;
        _fundingTransactionFactory = new FundingTransactionFactory(feeService);
        _commitmentTransactionFactory = new CommitmentTransactionFactory(feeService);
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingFundingTransaction_Then_ShouldBeEqualToTestVector()
    {
        // Given
        var fundingInputCoin = new Coin(AppendixBVectors.INPUT_TX, AppendixBVectors.INPUT_INDEX);

        // When
        var fundingTransaction = _fundingTransactionFactory.CreateFundingTransaction(
            AppendixBVectors.LOCAL_PUB_KEY,
            AppendixBVectors.REMOTE_PUB_KEY,
            AppendixBVectors.FUNDING_SATOSHIS,
            AppendixBVectors.CHANGE_SCRIPT.PaymentScript,
            AppendixBVectors.CHANGE_SCRIPT,
            [fundingInputCoin],
            new BitcoinSecret(AppendixBVectors.INPUT_SIGNING_PRIV_KEY, ConfigManager.Instance.Network));
        var finalFundingTx = fundingTransaction.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixBVectors.EXPECTED_TX.ToBytes(), finalFundingTx.ToBytes());
    }

    [Fact]
    public void Given_Bolt3Specifications_When_CreatingCommitmentTransaction_Then_ShouldBeEqualToTestVector()
    {
        // Given
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
        var commitmentTransacion = _commitmentTransactionFactory.CreateCommitmentTransaction(fundingOutput.ToCoin(),
            AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
            AppendixCVectors.NODE_B_PAYMENT_BASEPOINT,
            AppendixCVectors.NODE_A_DELAYED_PUBKEY,
            AppendixCVectors.NODE_A_REVOCATION_PUBKEY,
            AppendixCVectors.TO_LOCAL_MSAT,
            AppendixCVectors.TO_REMOTE_MSAT,
            AppendixCVectors.LOCAL_DELAY,
            commitmentNumber, true,
            new BitcoinSecret(
                AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                ConfigManager.Instance.Network));

        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE, fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Then
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_1.ToBytes(), finalCommitmentTx.ToBytes());
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
    public void Dispose()
    {
        ConfigManager.Instance.IsOptionAnchorOutput = _previousIsOptionAnchorOutput;
    }
}