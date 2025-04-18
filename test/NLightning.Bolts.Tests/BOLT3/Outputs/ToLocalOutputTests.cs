using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Outputs;

using Bolts.BOLT3.Outputs;
using Common.Types;

public class ToLocalOutputTests
{
    private readonly PubKey _localDelayedPubKey = new PubKey("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");
    private readonly PubKey _revocationPubKey = new PubKey("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    private readonly uint _toSelfDelay = 144; // Typical delay value (1 day)
    private readonly LightningMoney _amount = new LightningMoney(1000000);

    [Fact]
    public void Given_ValidParameters_When_ConstructingToLocalOutput_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var toLocalOutput = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, _toSelfDelay, _amount);

        // Then
        Assert.Equal(_localDelayedPubKey, toLocalOutput.LocalDelayedPubKey);
        Assert.Equal(_revocationPubKey, toLocalOutput.RevocationPubKey);
        Assert.Equal(_toSelfDelay, toLocalOutput.ToSelfDelay);
        Assert.Equal(_amount, toLocalOutput.Amount);
        Assert.Equal(ScriptType.P2WSH, toLocalOutput.ScriptType);
        Assert.NotNull(toLocalOutput.RedeemScript);
        Assert.NotNull(toLocalOutput.ScriptPubKey);
    }

    [Fact]
    public void Given_ValidParameters_When_ConstructingToLocalOutput_Then_GeneratesCorrectScript()
    {
        // Given
        const string EXPECTED_TO_SELF_DELAY = "9000";

        // When
        var toLocalOutput = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, _toSelfDelay, _amount);
        var redeemScript = toLocalOutput.RedeemScript;

        // Then
        // Check basic script structure
        var scriptString = redeemScript.ToString();
        Assert.Contains("OP_IF", scriptString);
        Assert.Contains("OP_ELSE", scriptString);
        Assert.Contains("OP_CSV", scriptString);
        Assert.Contains("OP_ENDIF", scriptString);
        Assert.Contains("OP_CHECKSIG", scriptString);

        // Check presence of key hex representations
        Assert.Contains(_localDelayedPubKey.ToHex(), scriptString);
        Assert.Contains(_revocationPubKey.ToHex(), scriptString);

        // Check toSelfDelay is included
        Assert.Contains(EXPECTED_TO_SELF_DELAY, scriptString);

        // Check if the script is a P2WSH
        Assert.True(toLocalOutput.ScriptPubKey.IsScriptType(ScriptType.P2WSH));
    }

    [Fact]
    public void Given_ToLocalOutput_When_ToCoinCalled_Then_ReturnsCorrectScriptCoin()
    {
        // Given
        var toLocalOutput = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, _toSelfDelay, _amount)
        {
            TxId = uint256.Parse("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"),
            Index = 1
        };

        // When
        var coin = toLocalOutput.ToCoin();

        // Then
        Assert.Equal(toLocalOutput.TxId, coin.Outpoint.Hash);
        Assert.Equal(toLocalOutput.Index, (int)coin.Outpoint.N);
        Assert.Equal((Money)toLocalOutput.Amount, coin.Amount);
        Assert.Equal(toLocalOutput.ScriptPubKey, coin.ScriptPubKey);
        Assert.Equal(toLocalOutput.RedeemScript, coin.Redeem);
    }

    [Fact]
    public void Given_ZeroAmount_When_ConstructingToLocalOutput_Then_CreatesZeroValueOutput()
    {
        // Given
        var zeroAmount = new LightningMoney(0);

        // When
        var toLocalOutput = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, _toSelfDelay, zeroAmount);

        // Then
        Assert.Equal(zeroAmount, toLocalOutput.Amount);
        Assert.Equal(Money.Zero, (Money)toLocalOutput.Amount);
    }

    [Fact]
    public void Given_DifferentInputs_When_ConstructingToLocalOutputs_Then_GeneratesDifferentScripts()
    {
        // Given
        var alternateDelayedPubKey = new PubKey("02f5559c428d3a3e3579adc6516fdb4d3be6fb96290f1a0b4f873a16fa4c397c07");
        var alternateRevocationPubKey = new PubKey("0279be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798");
        var alternateDelay = 432u; // 3 days

        // When
        var toLocalOutput1 = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, _toSelfDelay, _amount);
        var toLocalOutput2 = new ToLocalOutput(alternateDelayedPubKey, _revocationPubKey, _toSelfDelay, _amount);
        var toLocalOutput3 = new ToLocalOutput(_localDelayedPubKey, alternateRevocationPubKey, _toSelfDelay, _amount);
        var toLocalOutput4 = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, alternateDelay, _amount);

        // Then
        Assert.NotEqual(toLocalOutput1.RedeemScript, toLocalOutput2.RedeemScript);
        Assert.NotEqual(toLocalOutput1.RedeemScript, toLocalOutput3.RedeemScript);
        Assert.NotEqual(toLocalOutput1.RedeemScript, toLocalOutput4.RedeemScript);
        Assert.NotEqual(toLocalOutput1.ScriptPubKey, toLocalOutput2.ScriptPubKey);
        Assert.NotEqual(toLocalOutput1.ScriptPubKey, toLocalOutput3.ScriptPubKey);
        Assert.NotEqual(toLocalOutput1.ScriptPubKey, toLocalOutput4.ScriptPubKey);
    }

    [Fact]
    public void Given_ToLocalOutputs_When_ComparingThem_Then_UsesTransactionOutputComparer()
    {
        // Given
        var output1 = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, _toSelfDelay, new LightningMoney(1000000));
        var output2 = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, _toSelfDelay, new LightningMoney(2000000));

        // When
        var comparison = output1.CompareTo(output2);

        // Then
        Assert.NotEqual(0, comparison); // The actual comparison is handled by TransactionOutputComparer
    }
}