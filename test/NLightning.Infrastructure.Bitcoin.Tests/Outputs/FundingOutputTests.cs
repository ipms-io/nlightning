// using NBitcoin;
//
// namespace NLightning.Infrastructure.Bitcoin.Tests.Outputs;
//
// using Bitcoin.Outputs;
// using Domain.Money;
//
// public class FundingOutputTests
// {
//     private readonly PubKey _localPubKey = new("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");
//     private readonly PubKey _remotePubKey = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
//     private readonly LightningMoney _amount = new(1000000);
//
//     [Fact]
//     public void Given_ValidParameters_When_ConstructingFundingOutput_Then_PropertiesAreSetCorrectly()
//     {
//         // Given
//
//         // When
//         var fundingOutput = new FundingOutput(_amount, _localPubKey, _remotePubKey);
//
//         // Then
//         Assert.Equal(_localPubKey, fundingOutput.LocalPubKey);
//         Assert.Equal(_remotePubKey, fundingOutput.RemotePubKey);
//         Assert.Equal(_amount, fundingOutput.Amount);
//         Assert.Equal(ScriptType.P2WSH, fundingOutput.ScriptType);
//         Assert.NotNull(fundingOutput.ScriptPubKey);
//     }
//
//     [Fact]
//     public void Given_IdenticalPubKeys_When_ConstructingFundingOutput_Then_ThrowsArgumentException()
//     {
//         // Given
//         var remotePubKey = _localPubKey; // Same as localPubKey
//
//         // When/Then
//         var exception = Assert.Throws<ArgumentException>(() => new FundingOutput(_amount, _localPubKey, remotePubKey));
//         Assert.Contains("Public keys must be different", exception.Message);
//     }
//
//     [Fact]
//     public void Given_ZeroAmount_When_ConstructingFundingOutput_Then_ThrowsArgumentException()
//     {
//         // Given
//         var amount = new LightningMoney(0);
//
//         // When/Then
//         var exception =
//             Assert.Throws<ArgumentOutOfRangeException>(() => new FundingOutput(amount, _localPubKey, _remotePubKey));
//         Assert.Contains("Funding amount must be greater than zero", exception.Message);
//     }
//
//     [Fact]
//     public void Given_ValidParameters_When_ConstructingFundingOutput_Then_CreatesCorrectMultisigScript()
//     {
//         // Given
//
//         // When
//         var fundingOutput = new FundingOutput(_amount, _localPubKey, _remotePubKey);
//         var redeemScriptString = new Script(fundingOutput.BitcoinScriptPubKey).ToString();
//
//         // Then
//         // Check that it's a 2-of-2 multisig script
//         Assert.StartsWith("2", redeemScriptString);
//         Assert.Contains(_localPubKey.ToHex(), redeemScriptString);
//         Assert.Contains(_remotePubKey.ToHex(), redeemScriptString);
//         Assert.EndsWith("2 OP_CHECKMULTISIG", redeemScriptString);
//
//         // Check if the script is a P2WSH
//         Assert.True(fundingOutput.ScriptPubKey.IsScriptType(ScriptType.P2WSH));
//     }
//
//     [Fact]
//     public void Given_FundingOutput_When_ToCoinCalled_Then_ReturnsCorrectScriptCoin()
//     {
//         // Given
//         var fundingOutput = new FundingOutput(_amount, _localPubKey, _remotePubKey)
//         {
//             TransactionId = Convert.FromHexString("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"),
//             Index = 1
//         };
//
//         // When
//         var coin = fundingOutput.ToCoin();
//
//         // Then
//         Assert.Equal(fundingOutput.TransactionId, coin.Outpoint.Hash);
//         Assert.Equal(fundingOutput.Index, (int)coin.Outpoint.N);
//         Assert.Equal((Money)fundingOutput.Amount, coin.Amount);
//         Assert.Equal(fundingOutput.ScriptPubKey, coin.ScriptPubKey);
//         Assert.Equal(fundingOutput.RedeemScript, coin.Redeem);
//     }
//
//     [Fact]
//     public void Given_TwoFundingOutputs_When_ComparingThem_Then_UsesTransactionOutputComparer()
//     {
//         // Given
//         var output1 = new FundingOutput(new LightningMoney(1000000), _localPubKey, _remotePubKey);
//         var output2 = new FundingOutput(new LightningMoney(2000000), _localPubKey, _remotePubKey);
//
//         // When
//         var comparison = output1.CompareTo(output2);
//
//         // Then
//         Assert.NotEqual(0, comparison); // The actual comparison is handled by TransactionOutputComparer
//     }
//
//     [Fact]
//     public void Given_DifferentPubKeyOrder_When_ConstructingFundingOutputs_Then_CreatesIdenticalRedeemScripts()
//     {
//         // Given
//         var fundingOutput1 = new FundingOutput(_amount, _localPubKey, _remotePubKey);
//         var fundingOutput2 = new FundingOutput(_amount, _remotePubKey, _localPubKey);
//
//         // When/Then
//         // The multisig script should order the keys lexicographically, so the scripts should be identical
//         Assert.Equal(fundingOutput1.BitcoinScriptPubKey, fundingOutput2.BitcoinScriptPubKey);
//         Assert.Equal(fundingOutput1.ScriptPubKey, fundingOutput2.ScriptPubKey);
//     }
// }