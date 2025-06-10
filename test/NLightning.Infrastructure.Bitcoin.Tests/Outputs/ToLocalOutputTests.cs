using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Tests.Outputs;

using Bitcoin.Outputs;
using Domain.Money;

public class ToLocalOutputTests
{
    private readonly PubKey _localDelayedPubKey =
        new("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");

    private readonly PubKey _revocationPubKey =
        new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");

    private const uint ToSelfDelay = 144; // Typical delay value (1 day)
    private readonly LightningMoney _amount = LightningMoney.Satoshis(1_000);

    [Fact]
    public void Given_ValidParameters_When_ConstructingToLocalOutput_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var toLocalOutput = new ToLocalOutput(_amount, _localDelayedPubKey, _revocationPubKey, ToSelfDelay);

        // Then
        Assert.Equal(_localDelayedPubKey, toLocalOutput.LocalDelayedPubKey);
        Assert.Equal(_revocationPubKey, toLocalOutput.RevocationPubKey);
        Assert.Equal(ToSelfDelay, toLocalOutput.ToSelfDelay);
        Assert.Equal(_amount, toLocalOutput.Amount);
        Assert.Equal(ScriptType.P2WSH, toLocalOutput.ScriptType);
        Assert.NotNull(toLocalOutput.ScriptPubKey);
    }

    [Fact]
    public void Given_ValidParameters_When_ConstructingToLocalOutput_Then_GeneratesCorrectScript()
    {
        // Given
        const string expectedToSelfDelay = "9000";

        // When
        var toLocalOutput = new ToLocalOutput(_amount, _localDelayedPubKey, _revocationPubKey, ToSelfDelay);
        var redeemScriptString = new Script(toLocalOutput.RedeemBitcoinScript).ToString();

        // Then
        // Check basic script structure
        Assert.Contains("OP_IF", redeemScriptString);
        Assert.Contains("OP_ELSE", redeemScriptString);
        Assert.Contains("OP_CSV", redeemScriptString);
        Assert.Contains("OP_ENDIF", redeemScriptString);
        Assert.Contains("OP_CHECKSIG", redeemScriptString);

        // Check presence of key hex representations
        Assert.Contains(_localDelayedPubKey.ToHex(), redeemScriptString);
        Assert.Contains(_revocationPubKey.ToHex(), redeemScriptString);

        // Check toSelfDelay is included
        Assert.Contains(expectedToSelfDelay, redeemScriptString);

        // Check if the script is a P2WSH
        Assert.True(toLocalOutput.ScriptPubKey.IsScriptType(ScriptType.P2WSH));
    }

    [Fact]
    public void Given_ToLocalOutput_When_ToCoinCalled_Then_ReturnsCorrectScriptCoin()
    {
        // Given
        var toLocalOutput = new ToLocalOutput(_amount, _localDelayedPubKey, _revocationPubKey, ToSelfDelay)
        {
            TransactionId = Convert.FromHexString("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"),
            Index = 1
        };

        // When
        var coin = toLocalOutput.ToCoin();

        // Then
        Assert.Equal(toLocalOutput.TransactionId, coin.Outpoint.Hash.ToBytes());
        Assert.Equal(toLocalOutput.Index, coin.Outpoint.N);
        Assert.Equal(toLocalOutput.Amount, LightningMoney.Satoshis(coin.Amount.Satoshi));
        Assert.Equal(toLocalOutput.ScriptPubKey, coin.ScriptPubKey);
        Assert.Equal(toLocalOutput.RedeemScript, coin.Redeem);
    }

    [Fact]
    public void Given_ZeroAmount_When_ConstructingToLocalOutput_Then_CreatesZeroValueOutput()
    {
        // Given
        var zeroAmount = new LightningMoney(0);

        // When
        var toLocalOutput = new ToLocalOutput(zeroAmount, _localDelayedPubKey, _revocationPubKey, ToSelfDelay);

        // Then
        Assert.Equal(zeroAmount, toLocalOutput.Amount);
        Assert.Equal(LightningMoney.Zero, toLocalOutput.Amount);
    }

    [Fact]
    public void Given_DifferentInputs_When_ConstructingToLocalOutputs_Then_GeneratesDifferentScripts()
    {
        // Given
        var alternateDelayedPubKey = new PubKey("02f5559c428d3a3e3579adc6516fdb4d3be6fb96290f1a0b4f873a16fa4c397c07");
        var alternateRevocationPubKey =
            new PubKey("0279be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798");
        const uint alternateDelay = 432u; // 3 days

        // When
        var toLocalOutput1 = new ToLocalOutput(_amount, _localDelayedPubKey, _revocationPubKey, ToSelfDelay);
        var toLocalOutput2 = new ToLocalOutput(_amount, alternateDelayedPubKey, _revocationPubKey, ToSelfDelay);
        var toLocalOutput3 = new ToLocalOutput(_amount, _localDelayedPubKey, alternateRevocationPubKey, ToSelfDelay);
        var toLocalOutput4 = new ToLocalOutput(_amount, _localDelayedPubKey, _revocationPubKey, alternateDelay);

        // Then
        Assert.NotEqual(toLocalOutput1.RedeemBitcoinScript, toLocalOutput2.RedeemBitcoinScript);
        Assert.NotEqual(toLocalOutput1.RedeemBitcoinScript, toLocalOutput3.RedeemBitcoinScript);
        Assert.NotEqual(toLocalOutput1.RedeemBitcoinScript, toLocalOutput4.RedeemBitcoinScript);
        Assert.NotEqual(toLocalOutput1.ScriptPubKey, toLocalOutput2.ScriptPubKey);
        Assert.NotEqual(toLocalOutput1.ScriptPubKey, toLocalOutput3.ScriptPubKey);
        Assert.NotEqual(toLocalOutput1.ScriptPubKey, toLocalOutput4.ScriptPubKey);
    }

    [Fact]
    public void Given_ToLocalOutputs_When_ComparingThem_Then_UsesTransactionOutputComparer()
    {
        // Given
        var output1 = new ToLocalOutput(LightningMoney.Satoshis(1_000), _localDelayedPubKey, _revocationPubKey,
                                        ToSelfDelay);
        var output2 = new ToLocalOutput(LightningMoney.Satoshis(2_000), _localDelayedPubKey, _revocationPubKey,
                                        ToSelfDelay);

        // When
        var comparison = output1.CompareTo(output2);

        // Then
        Assert.NotEqual(0, comparison); // The actual comparison is handled by TransactionOutputComparer
    }
}