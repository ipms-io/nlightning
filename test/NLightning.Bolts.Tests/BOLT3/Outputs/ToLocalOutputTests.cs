using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Outputs;

using Bolts.BOLT3.Outputs;
using Common.Types;

public class ToLocalOutputTests
{
    private readonly PubKey _localDelayedPubKey = new("03fd5960528dc152014952efdb702a88f71e3c1653b2314431701ec77e57fde83c");
    private readonly PubKey _revocationPubKey = new("0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19");
    private readonly LightningMoney _amountMilliSats = 1000000UL;
    private const uint TO_SELF_DELAY = 144;

    [Fact]
    public void Given_ValidParameters_When_CreatingToLocalOutput_Then_ShouldCreateSuccessfully()
    {
        // Given
        // (Parameters already initialized)

        // When
        var toLocalOutput = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, TO_SELF_DELAY, _amountMilliSats);

        // Then
        Assert.Equal(_amountMilliSats, toLocalOutput.AmountMilliSats);
        Assert.Equal(_localDelayedPubKey, toLocalOutput.LocalDelayedPubKey);
        Assert.Equal(_revocationPubKey, toLocalOutput.RevocationPubKey);
        Assert.Equal(TO_SELF_DELAY, toLocalOutput.ToSelfDelay);
    }

    [Fact]
    public void Given_ValidToLocalOutput_When_CallingToTxOut_Then_ShouldReturnValidTxOut()
    {
        // Given
        var expectedScriptPubKey = Convert.FromHexString("00204ADB4E2F00643DB396DD120D4E7DC17625F5F2C11A40D857ACCC862D6B7DD80E");
        var toLocalOutput = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, TO_SELF_DELAY, _amountMilliSats);

        // When
        var txOut = toLocalOutput.ToTxOut();

        // Then
        Assert.Equal(_amountMilliSats.Satoshi, txOut.Value.Satoshi);
        Assert.Equal(expectedScriptPubKey, txOut.ScriptPubKey.ToBytes());
    }
}