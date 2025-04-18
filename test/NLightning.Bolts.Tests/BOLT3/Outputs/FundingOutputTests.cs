using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Outputs;

using Bolts.BOLT3.Outputs;
using Common.Types;

public class FundingOutputTests
{
    private readonly PubKey _localPubKey = new("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");
    private readonly PubKey _remotePubKey = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    private readonly LightningMoney _amount = new(1000000);

    [Fact]
    public void Given_ValidParameters_When_ConstructingFundingOutput_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var fundingOutput = new FundingOutput(_localPubKey, _remotePubKey, _amount);

        // Then
        Assert.Equal(_localPubKey, fundingOutput.LocalPubKey);
        Assert.Equal(_remotePubKey, fundingOutput.RemotePubKey);
        Assert.Equal(_amount, fundingOutput.Amount);
        Assert.Equal(ScriptType.P2WSH, fundingOutput.ScriptType);
        Assert.NotNull(fundingOutput.RedeemScript);
        Assert.NotNull(fundingOutput.ScriptPubKey);
    }

    [Fact]
    public void Given_IdenticalPubKeys_When_ConstructingFundingOutput_Then_ThrowsArgumentException()
    {
        // Given
        var remotePubKey = _localPubKey; // Same as localPubKey

        // When/Then
        var exception = Assert.Throws<ArgumentException>(() => new FundingOutput(_localPubKey, remotePubKey, _amount));
        Assert.Contains("Public keys must be different", exception.Message);
    }

    [Fact]
    public void Given_ZeroAmount_When_ConstructingFundingOutput_Then_ThrowsArgumentException()
    {
        // Given
        var amount = new LightningMoney(0);

        // When/Then
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new FundingOutput(_localPubKey, _remotePubKey, amount));
        Assert.Contains("Funding amount must be greater than zero", exception.Message);
    }

    [Fact]
    public void Given_ValidParameters_When_ConstructingFundingOutput_Then_CreatesCorrectMultisigScript()
    {
        // Given

        // When
        var fundingOutput = new FundingOutput(_localPubKey, _remotePubKey, _amount);
        var redeemScript = fundingOutput.RedeemScript;

        // Then
        // Check that it's a 2-of-2 multisig script
        Assert.StartsWith("2", redeemScript.ToString());
        Assert.Contains(_localPubKey.ToHex(), redeemScript.ToString());
        Assert.Contains(_remotePubKey.ToHex(), redeemScript.ToString());
        Assert.EndsWith("2 OP_CHECKMULTISIG", redeemScript.ToString());

        // Check if the script is a P2WSH
        Assert.True(fundingOutput.ScriptPubKey.IsScriptType(ScriptType.P2WSH));
    }

    [Fact]
    public void Given_FundingOutput_When_ToCoinCalled_Then_ReturnsCorrectScriptCoin()
    {
        // Given
        var fundingOutput = new FundingOutput(_localPubKey, _remotePubKey, _amount)
        {
            TxId = uint256.Parse("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"),
            Index = 1
        };

        // When
        var coin = fundingOutput.ToCoin();

        // Then
        Assert.Equal(fundingOutput.TxId, coin.Outpoint.Hash);
        Assert.Equal(fundingOutput.Index, (int)coin.Outpoint.N);
        Assert.Equal((Money)fundingOutput.Amount, coin.Amount);
        Assert.Equal(fundingOutput.ScriptPubKey, coin.ScriptPubKey);
        Assert.Equal(fundingOutput.RedeemScript, coin.Redeem);
    }

    [Fact]
    public void Given_TwoFundingOutputs_When_ComparingThem_Then_UsesTransactionOutputComparer()
    {
        // Given
        var output1 = new FundingOutput(_localPubKey, _remotePubKey, new LightningMoney(1000000));
        var output2 = new FundingOutput(_localPubKey, _remotePubKey, new LightningMoney(2000000));

        // When
        var comparison = output1.CompareTo(output2);

        // Then
        Assert.NotEqual(0, comparison); // The actual comparison is handled by TransactionOutputComparer
    }

    [Fact]
    public void Given_DifferentPubKeyOrder_When_ConstructingFundingOutputs_Then_CreatesIdenticalRedeemScripts()
    {
        // Given
        var fundingOutput1 = new FundingOutput(_localPubKey, _remotePubKey, _amount);
        var fundingOutput2 = new FundingOutput(_remotePubKey, _localPubKey, _amount);

        // When/Then
        // The multisig script should order the keys lexicographically, so the scripts should be identical
        Assert.Equal(fundingOutput1.RedeemScript, fundingOutput2.RedeemScript);
        Assert.Equal(fundingOutput1.ScriptPubKey, fundingOutput2.ScriptPubKey);
    }
}