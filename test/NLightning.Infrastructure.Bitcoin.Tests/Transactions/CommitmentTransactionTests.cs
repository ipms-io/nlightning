// using NBitcoin;
// using NBitcoin.Crypto;
// using NLightning.Domain.Protocol.ValueObjects;
// using NLightning.Infrastructure.Crypto.Hashes;
//
// namespace NLightning.Infrastructure.Bitcoin.Tests.Transactions;
//
// using Bitcoin.Outputs;
// using Bitcoin.Transactions;
// using Domain.Enums;
// using Domain.Money;
// using Protocol.Models;
//
// public class CommitmentTransactionTests
// {
//     private const uint ToSelfDelay = 144;
//
//     private readonly PubKey _localFundingPubKey =
//         new("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb");
//
//     private readonly PubKey _remoteFundingPubKey =
//         new("030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c1");
//
//     private readonly PubKey _localPaymentBasepoint =
//         new("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");
//
//     private readonly PubKey _remotePaymentBasepoint =
//         new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
//
//     private readonly PubKey _localDelayedPubKey =
//         new("03fd5960528dc152014952efdb702a88f71e3c1653b2314431701ec77e57fde83c");
//
//     private readonly PubKey _revocationPubKey =
//         new("0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19");
//
//     private readonly FundingOutput _fundingOutput;
//     private readonly LightningMoney _toLocalAmount = new(8_000, LightningMoneyUnit.Satoshi);
//     private readonly LightningMoney _toRemoteAmount = new(2_000, LightningMoneyUnit.Satoshi);
//     private readonly CommitmentNumber _commitmentNumber;
//
//     private readonly BitcoinSecret _privateKey =
//         new(new Key(Convert.FromHexString("6bd078650fcee8444e4e09825227b801a1ca928debb750eb36e6d56124bb20e8")),
//             NBitcoin.Network.TestNet);
//
//     private readonly LightningMoney _defaultDustLimitAmount = LightningMoney.Satoshis(540);
//     private readonly Sha256 _sha256 = new();
//
//     public CommitmentTransactionTests()
//     {
//         _fundingOutput =
//             new FundingOutput(LightningMoney.Satoshis(1_000_000), _localFundingPubKey, _remoteFundingPubKey)
//             {
//                 TransactionId =
//                     Convert.FromHexString("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"),
//                 Index = 0
//             };
//
//         _commitmentNumber =
//             new CommitmentNumber(_localPaymentBasepoint.ToBytes(), _remotePaymentBasepoint.ToBytes(), _sha256, 42);
//     }
//
//     [Fact]
//     public void
//         Given_ValidParametersAsChannelFunder_When_ConstructingCommitmentTransaction_Then_PropertiesAreSetCorrectly()
//     {
//         // Given
//         // When
//         var commitmentTx = CreateCommitmentTransaction(true, LightningMoney.Zero, _defaultDustLimitAmount);
//
//         // Then
//         Assert.Equal(_commitmentNumber, commitmentTx.CommitmentNumber);
//
//         Assert.NotNull(commitmentTx.ToLocalOutput);
//         Assert.NotNull(commitmentTx.ToRemoteOutput);
//         Assert.Null(commitmentTx.LocalAnchorOutput);
//         Assert.Null(commitmentTx.RemoteAnchorOutput);
//
//         Assert.Equal(LightningMoney.Zero, commitmentTx.ToLocalOutput.Amount);
//         Assert.Equal(_toRemoteAmount, commitmentTx.ToRemoteOutput.Amount);
//     }
//
//     [Fact]
//     public void Given_ValidParametersAsNonFunder_When_ConstructingCommitmentTransaction_Then_PropertiesAreSetCorrectly()
//     {
//         // Given
//         // When
//         var commitmentTx = CreateCommitmentTransaction(false, LightningMoney.Zero, _defaultDustLimitAmount);
//
//         // Then
//         Assert.Equal(_commitmentNumber, commitmentTx.CommitmentNumber);
//
//         Assert.NotNull(commitmentTx.ToLocalOutput);
//         Assert.NotNull(commitmentTx.ToRemoteOutput);
//         Assert.Null(commitmentTx.LocalAnchorOutput);
//         Assert.Null(commitmentTx.RemoteAnchorOutput);
//
//         Assert.Equal(_toLocalAmount, commitmentTx.ToLocalOutput.Amount);
//         Assert.Equal(LightningMoney.Zero, commitmentTx.ToRemoteOutput.Amount);
//     }
//
//     [Fact]
//     public void Given_AnchorOutputsEnabled_When_ConstructingCommitmentTransaction_Then_CreatesAnchorOutputs()
//     {
//         // Given
//         var expectedAnchorAmount = LightningMoney.Satoshis(330);
//
//         // When
//         var commitmentTx = CreateCommitmentTransaction(true, expectedAnchorAmount, _defaultDustLimitAmount);
//
//         // Then
//         Assert.NotNull(commitmentTx.LocalAnchorOutput);
//         Assert.NotNull(commitmentTx.RemoteAnchorOutput);
//         Assert.Equal(expectedAnchorAmount, commitmentTx.LocalAnchorOutput.Amount);
//         Assert.Equal(expectedAnchorAmount, commitmentTx.RemoteAnchorOutput.Amount);
//     }
//
//     [Fact]
//     public void Given_ZeroAmounts_When_ConstructingCommitmentTransaction_Then_ThrowsArgumentException()
//     {
//         // Given
//
//         // When/Then
//         var exception = Assert.Throws<ArgumentException>(() =>
//         {
//             return new CommitmentTransaction(LightningMoney.Zero, LightningMoney.Satoshis(800), false, Network.Main,
//                                              _fundingOutput, _localPaymentBasepoint, _remotePaymentBasepoint,
//                                              _localDelayedPubKey, _revocationPubKey, LightningMoney.Zero,
//                                              LightningMoney.Zero, ToSelfDelay, _commitmentNumber, true);
//         });
//
//         Assert.Contains("Both toLocalAmount and toRemoteAmount cannot be zero", exception.Message);
//     }
//
//     [Fact]
//     public void Given_AmountBelowDustLimitForFunder_When_SigningTransaction_Then_RemovesToLocalOutput()
//     {
//         // Given
//         // ToLocalAmount and ToRemoteAmount are inverted to simulate the dust limit
//         var commitmentTx = new CommitmentTransaction(LightningMoney.Zero, LightningMoney.Satoshis(800), false,
//                                                      Network.Main, _fundingOutput, _localPaymentBasepoint,
//                                                      _remotePaymentBasepoint, _localDelayedPubKey, _revocationPubKey,
//                                                      _toRemoteAmount, _toLocalAmount, ToSelfDelay, _commitmentNumber,
//                                                      true);
//
//         commitmentTx.ConstructTransaction(new LightningMoney(2_000, LightningMoneyUnit.Satoshi));
//
//         // When
//         commitmentTx.SignTransaction(_privateKey);
//         var signedTx = commitmentTx.GetSignedTransaction();
//
//         // Then
//         Assert.Single(signedTx.Outputs); // Only toRemote output remains
//         Assert.Equal(LightningMoney.Zero, commitmentTx.ToLocalOutput.Amount); // ToLocal output was removed
//         Assert.Equal(commitmentTx.TxId, commitmentTx.ToRemoteOutput.TxId);
//     }
//
//     [Fact]
//     public void Given_AmountBelowDustLimitForNonFunder_When_SigningTransaction_Then_RemovesToRemoteOutput()
//     {
//         // Given
//         var commitmentTx = CreateCommitmentTransaction(false, LightningMoney.Zero, LightningMoney.Satoshis(800));
//         commitmentTx.ConstructTransaction(new LightningMoney(2_000, LightningMoneyUnit.Satoshi));
//
//         // When
//         commitmentTx.SignTransaction(_privateKey);
//         var signedTx = commitmentTx.GetSignedTransaction();
//
//         // Then
//         Assert.Single(signedTx.Outputs); // Only toLocal output remains
//         Assert.Equal(LightningMoney.Zero, commitmentTx.ToRemoteOutput.Amount); // ToRemote output was removed
//         Assert.Equal(commitmentTx.TxId, commitmentTx.ToLocalOutput.TxId);
//     }
//
//     [Fact]
//     public void Given_AddedHtlcOutputs_When_SigningTransaction_Then_OutputsHaveCorrectProperties()
//     {
//         // Given
//         var commitmentTx = CreateCommitmentTransaction(true, LightningMoney.Zero, _defaultDustLimitAmount);
//
//         var htlcOffered = new OfferedHtlcOutput(LightningMoney.Zero, _localPaymentBasepoint, _revocationPubKey,
//                                                 _localPaymentBasepoint, new ReadOnlyMemory<byte>([0]),
//                                                 new LightningMoney(500, LightningMoneyUnit.Satoshi), 500);
//         var htlcReceived = new ReceivedHtlcOutput(LightningMoney.Zero, _localPaymentBasepoint, _revocationPubKey,
//                                                   _localPaymentBasepoint, new ReadOnlyMemory<byte>([0]),
//                                                   new LightningMoney(400, LightningMoneyUnit.Satoshi), 500);
//
//         commitmentTx.AddOfferedHtlcOutput(htlcOffered);
//         commitmentTx.AddReceivedHtlcOutput(htlcReceived);
//
//         commitmentTx.ConstructTransaction(new LightningMoney(100, LightningMoneyUnit.Satoshi));
//
//         // When
//         commitmentTx.SignTransaction(_privateKey);
//         var signedTx = commitmentTx.GetSignedTransaction();
//
//         // Then
//         Assert.Equal(4, signedTx.Outputs.Count); // to_local, to_remote, offered_htlc, received_htlc
//         Assert.Single(commitmentTx.OfferedHtlcOutputs);
//         Assert.Equal(commitmentTx.TxId, commitmentTx.OfferedHtlcOutputs[0].TxId);
//         Assert.Single(commitmentTx.ReceivedHtlcOutputs);
//         Assert.Equal(commitmentTx.TxId, commitmentTx.ReceivedHtlcOutputs[0].TxId);
//     }
//
//     [Fact]
//     public void Given_UnsignedTransaction_When_GetSignedTransactionCalled_Then_ThrowsInvalidOperationException()
//     {
//         // Given
//         var commitmentTx = CreateCommitmentTransaction(true, LightningMoney.Satoshis(330), _defaultDustLimitAmount);
//
//         // When/Then
//         Assert.Throws<InvalidOperationException>(() => commitmentTx.GetSignedTransaction());
//     }
//
//     [Fact]
//     public void Given_SignedTransaction_When_GetSignedTransactionCalled_Then_ReturnsFinalizedTransaction()
//     {
//         // Given
//         var commitmentTx = CreateCommitmentTransaction(true, LightningMoney.Satoshis(330), _defaultDustLimitAmount);
//         commitmentTx.ConstructTransaction(new LightningMoney(1_000, LightningMoneyUnit.Satoshi));
//         commitmentTx.SignTransaction(_privateKey);
//
//         // When
//         var signedTx = commitmentTx.GetSignedTransaction();
//
//         // Then
//         Assert.NotNull(signedTx);
//         Assert.Equal(commitmentTx.TxId, signedTx.GetHash());
//     }
//
//     [Fact]
//     public void Given_RemoteSignature_When_AppendRemoteSignatureAndSign_Then_SignsTransaction()
//     {
//         // Given
//         var commitmentTx = CreateCommitmentTransaction(true, LightningMoney.Satoshis(330), _defaultDustLimitAmount);
//         var remoteSignature = new ECDSASignature(Convert.FromHexString(
//                                                      "3045022100c3127b33dcc741dd6b05b1e63cbd1a9a7d816f37af9b6756fa2376b056f032370220408b96279808fe57eb7e463710804cdf4f108388bc5cf722d8c848d2c7f9f3b0"));
//
//         // When
//         commitmentTx.AppendRemoteSignatureAndSign(remoteSignature, _remotePaymentBasepoint);
//         var transaction = commitmentTx.GetSignedTransaction();
//
//         // Then
//         Assert.NotNull(transaction);
//     }
//
//     private CommitmentTransaction CreateCommitmentTransaction(bool isChannelFunder, LightningMoney anchorAmount,
//                                                               LightningMoney dustLimitAmount)
//     {
//         return new CommitmentTransaction(anchorAmount, dustLimitAmount, false, Network.Main, _fundingOutput,
//                                          _localPaymentBasepoint, _remotePaymentBasepoint, _localDelayedPubKey,
//                                          _revocationPubKey, _toLocalAmount, _toRemoteAmount, ToSelfDelay,
//                                          _commitmentNumber, isChannelFunder);
//     }
// }

