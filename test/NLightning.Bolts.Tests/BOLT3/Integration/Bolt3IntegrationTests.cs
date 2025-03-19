using NBitcoin;
using NLightning.Bolts.BOLT3.Types;
using NLightning.Common.Enums;
using NLightning.Common.Types;

namespace NLightning.Bolts.Tests.BOLT3.Integration;

using Bolts.BOLT3.Calculators;
using Bolts.BOLT3.Factories;
using Common.Interfaces;
using Common.Managers;

public class Bolt3IntegrationTests
{
    private readonly FeeCalculator _feeCalculator;
    private readonly FundingTransactionFactory _fundingTransactionFactory;
    private readonly CommitmentTransactionFactory _commitmentTransactionFactory;
    private readonly IFeeService _feeService;

    public Bolt3IntegrationTests()
    {
        // Set Anchor = OFF
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        var feeRatePerKw = new LightningMoney(15000, LightningMoneyUnit.SATOSHI); // From test vector
        var feeServiceMock = new Mock<IFeeService>();
        feeServiceMock.Setup(x => x.GetCachedFeeRatePerKw()).Returns(feeRatePerKw);

        _feeService = feeServiceMock.Object;
        _feeCalculator = new FeeCalculator(_feeService);
        _fundingTransactionFactory = new FundingTransactionFactory(_feeCalculator);
        _commitmentTransactionFactory = new CommitmentTransactionFactory(_feeCalculator);
    }

    [Fact]
    public void IntegrationTest_ShouldFollowBolt3Specifications()
    {
        // Assert that we have the right input transaction
        Assert.Equal(AppendixBVectors.INPUT_TX_ID, AppendixBVectors.INPUT_TX.GetHash());

        // Create coin from the test vector
        var fundingInputCoin = new Coin(AppendixBVectors.INPUT_TX, AppendixBVectors.INPUT_INDEX);
        Assert.True(fundingInputCoin.ScriptPubKey.IsScriptType(ScriptType.P2PKH));

        // Create funding transaction
        var fundingTransaction = _fundingTransactionFactory.CreateFundingTransaction(
            AppendixBVectors.LOCAL_PUB_KEY,
            AppendixBVectors.REMOTE_PUB_KEY,
            AppendixBVectors.FUNDING_SATOSHIS,
            AppendixBVectors.CHANGE_SCRIPT.PaymentScript,
            AppendixBVectors.CHANGE_SCRIPT,
            [fundingInputCoin],
            new BitcoinSecret(AppendixBVectors.INPUT_SIGNING_PRIV_KEY, ConfigManager.Instance.Network));
        var finalFundingTx = fundingTransaction.GetSignedTransaction();

        // Verify the tx bytes are correct
        Assert.Equal(AppendixBVectors.EXPECTED_TX.ToBytes(), finalFundingTx.ToBytes());

        // Verify output amounts
        Assert.Equal(AppendixBVectors.EXPECTED_CHANGE_SATOSHIS.Satoshi, fundingTransaction.ChangeOutput.Amount.Satoshi);
        Assert.Equal(AppendixBVectors.FUNDING_SATOSHIS.Satoshi, fundingTransaction.FundingOutput.Amount.Satoshi);

        Assert.Equal(AppendixBVectors.EXPECTED_TX_ID, finalFundingTx.GetHash());

        var fundingOutput = fundingTransaction.FundingOutput;

        // Created a commitment number for the channel
        var commitmentNumber = new CommitmentNumber(AppendixCVectors.NODE_A_PAYMENT_BASEPOINT,
                                                    AppendixCVectors.NODE_B_PAYMENT_BASEPOINT,
                                                    AppendixCVectors.COMMITMENT_NUMBER);
        Assert.Equal(AppendixCVectors.EXPECTED_OBSCURING_FACTOR, commitmentNumber.ObscuringFactor);

        // Create commitment transaction for Node A
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
        // Add remote signature
        commitmentTransacion.AppendRemoteSignatureAndSign(AppendixCVectors.NODE_B_SIGNATURE, fundingOutput.RemotePubKey);

        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Validate commitment transaction outputs
        Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_1.ToBytes(), finalCommitmentTx.ToBytes());

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
}