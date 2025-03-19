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
                                                                                             AppendixCVectors.NODE_A_FUNDING_PUBKEY,
                                                                                             AppendixCVectors.NODE_B_FUNDING_PUBKEY,
                                                                                             AppendixCVectors.NODE_A_DELAYED_PUBKEY,
                                                                                             AppendixCVectors.NODE_A_REVOCATION_PUBKEY,
                                                                                             AppendixCVectors.TO_LOCAL_MSAT,
                                                                                             AppendixCVectors.TO_REMOTE_MSAT,
                                                                                             AppendixCVectors.LOCAL_DELAY,
                                                                                             commitmentNumber, true,
                                                                                             new BitcoinSecret(
                                                                                                 AppendixCVectors.NODE_A_FUNDING_PRIVKEY,
                                                                                                 ConfigManager.Instance.Network));
        var finalCommitmentTx = commitmentTransacion.GetSignedTransaction();

        // Validate commitment transaction outputs
        Assert.Equal(2, finalCommitmentTx.Outputs.Count);
        Assert.Equal(AppendixCVectors.EXPECTED_TO_LOCAL_WIT_SCRIPT_1, commitmentTransacion.ToLocalOutput?.RedeemScript.ToBytes());
        // 02 000000 01     bef67e4e2fb9ddeeb3461973cd4c62abb35050b1add772995b820b584a488489 000000004b00483045022100b01341632124319594cb1e3c01c752f6b487158f1bc1e6ae6ae433470b4d4bb9022055732fb6b26acd47558b0dcadd10a8023b2cf2bc90a98a70d7b8caa121c653fe010038b02b8002c0c62d000000000022002077abe0c6e4735b7a9858dc82bb7ec4e6889532e356607095f5ba685b58a7f9abd1a06a00000000002200204adb4e2f00643db396dd120d4e7dc17625f5f2c11a40d857accc862d6b7dd80e3e195220
        // 02 00000000 0101 bef67e4e2fb9ddeeb3461973cd4c62abb35050b1add772995b820b584a488489 000000000038b02b8002c0c62d0000000000160014cc1b07838e387deacd0e5232e1e8b49f4c29e48454a56a00000000002200204adb4e2f00643db396dd120d4e7dc17625f5f2c11a40d857accc862d6b7dd80e04004730440220616210b2cc4d3afb601013c373bbd8aac54febd9f15400379a8cb65ce7deca60022034236c010991beb7ff770510561ae8dc885b8d38d1947248c38f2ae05564714201483045022100c3127b33dcc741dd6b05b1e63cbd1a9a7d816f37af9b6756fa2376b056f032370220408b96279808fe57eb7e463710804cdf4f108388bc5cf722d8c848d2c7f9f3b001475221023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb21030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c152ae3e195220
        // Assert.Equal(AppendixCVectors.EXPECTED_COMMIT_TX_1, finalCommitmentTx);

        // Validate the initial commitment transaction for Node A
        // - Ensure the transaction adheres to BOLT 3 specifications.

        // Initial Commitment Transaction Construction for Node B
        // - Construct the initial commitment transaction for Node B.

        // Validate the initial commitment transaction for Node B
        // - Ensure the transaction adheres to BOLT 3 specifications.

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