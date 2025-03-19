using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Outputs;

using Bolts.BOLT3.Outputs;

public class ToLocalOutputTests
{
    private readonly PubKey _localDelayedPubKey = new("03fd5960528dc152014952efdb702a88f71e3c1653b2314431701ec77e57fde83c");
    private readonly PubKey _revocationPubKey = new("0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19");
    private const uint TO_SELF_DELAY = 144;
    private const long AMOUNT_SATS = 1000;

    [Fact]
    public void Given_ValidParameters_When_CreatingToLocalOutput_Then_ShouldCreateSuccessfully()
    {
        // Given
        // (Parameters already initialized)

        // When
        var toLocalOutput = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, TO_SELF_DELAY, AMOUNT_SATS);

        // Then
        Assert.Equal(AMOUNT_SATS, toLocalOutput.Amount.Satoshi);
        Assert.Equal(_localDelayedPubKey, toLocalOutput.LocalDelayedPubKey);
        Assert.Equal(_revocationPubKey, toLocalOutput.RevocationPubKey);
        Assert.Equal(TO_SELF_DELAY, toLocalOutput.ToSelfDelay);
    }

    [Fact]
    public void Given_ValidToLocalOutput_When_CallingToTxOut_Then_ShouldReturnValidTxOut()
    {
        // Given
        var expectedScriptPubKey = Convert.FromHexString("63210212A140CD0C6539D07CD08DFE09984DEC3251EA808B892EFEAC3EDE9402BF2B1967029000B2752103FD5960528DC152014952EFDB702A88F71E3C1653B2314431701EC77E57FDE83C68AC");
        var toLocalOutput = new ToLocalOutput(_localDelayedPubKey, _revocationPubKey, TO_SELF_DELAY, AMOUNT_SATS);

        // When
        var txOut = toLocalOutput.ToTxOut();

        // Then
        Assert.Equal(AMOUNT_SATS, txOut.Value.Satoshi);
        Assert.Equal(expectedScriptPubKey, txOut.ScriptPubKey.ToBytes());
    }
}