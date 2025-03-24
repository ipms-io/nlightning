using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Outputs;

using Bolts.BOLT3.Outputs;
using Common.Managers;
using Common.Types;

public class ToRemoteOutputTests
{
    private readonly PubKey _remotePubKey = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    private readonly LightningMoney _amount = 1000000UL;

    [Fact]
    public void Given_ValidParameters_When_ConstructingToRemoteOutput_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var toRemoteOutput = new ToRemoteOutput(_remotePubKey, _amount);

        // Then
        Assert.Equal(_remotePubKey, toRemoteOutput.RemotePubKey);
        Assert.Equal(_amount, toRemoteOutput.Amount);
        Assert.NotNull(toRemoteOutput.RedeemScript);
        Assert.NotNull(toRemoteOutput.ScriptPubKey);
    }

    [Fact]
    public void Given_NullRemotePubKey_When_ConstructingToRemoteOutput_Then_ThrowsArgumentNullException()
    {
        // Given
        PubKey remotePubKey = null;

        // When/Then
        Assert.Throws<ArgumentNullException>(() => new ToRemoteOutput(remotePubKey, _amount));
    }

    [Fact]
    public void Given_OptionAnchorOutputFalse_When_ConstructingToRemoteOutput_Then_UsesP2WPKH()
    {
        // Given
        var previousIsOptionAnchorOutput = ConfigManager.Instance.IsOptionAnchorOutput;
        ConfigManager.Instance.IsOptionAnchorOutput = false;

        try
        {
            // When
            var toRemoteOutput = new ToRemoteOutput(_remotePubKey, _amount);

            // Then
            Assert.Equal(ScriptType.P2WPKH, toRemoteOutput.ScriptType);
            Assert.Equal(_remotePubKey.WitHash.ScriptPubKey, toRemoteOutput.RedeemScript);
        }
        finally
        {
            ConfigManager.Instance.IsOptionAnchorOutput = previousIsOptionAnchorOutput;
        }
    }

    [Fact]
    public void Given_OptionAnchorOutputTrue_When_ConstructingToRemoteOutput_Then_UsesP2WSH()
    {
        // Given
        var previousIsOptionAnchorOutput = ConfigManager.Instance.IsOptionAnchorOutput;
        ConfigManager.Instance.IsOptionAnchorOutput = true;

        try
        {
            // When
            var toRemoteOutput = new ToRemoteOutput(_remotePubKey, _amount);

            // Then
            Assert.Equal(ScriptType.P2WSH, toRemoteOutput.ScriptType);

            // Check script structure
            var scriptString = toRemoteOutput.RedeemScript.ToString();
            Assert.Contains(_remotePubKey.ToHex(), scriptString);
            Assert.Contains("CHECKSIGVERIFY", scriptString);
            Assert.Contains("CSV", scriptString);
        }
        finally
        {
            ConfigManager.Instance.IsOptionAnchorOutput = previousIsOptionAnchorOutput;
        }
    }

    [Fact]
    public void Given_ToRemoteOutput_When_ToCoinCalled_Then_ReturnsCorrectScriptCoin()
    {
        // Given
        var toRemoteOutput = new ToRemoteOutput(_remotePubKey, _amount)
        {
            TxId = uint256.Parse("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"),
            Index = 1
        };

        // When
        var coin = toRemoteOutput.ToCoin();

        // Then
        Assert.Equal(toRemoteOutput.TxId, coin.Outpoint.Hash);
        Assert.Equal(toRemoteOutput.Index, coin.Outpoint.N);
        Assert.Equal((Money)toRemoteOutput.Amount, coin.Amount);
        Assert.Equal(toRemoteOutput.ScriptPubKey, coin.ScriptPubKey);
        Assert.Equal(toRemoteOutput.RedeemScript, coin.Redeem);
    }

    [Fact]
    public void Given_ZeroAmount_When_ConstructingToRemoteOutput_Then_CreatesZeroValueOutput()
    {
        // Given
        var zeroAmount = new LightningMoney(0);

        // When
        var toRemoteOutput = new ToRemoteOutput(_remotePubKey, zeroAmount);

        // Then
        Assert.Equal(zeroAmount, toRemoteOutput.Amount);
        Assert.Equal(Money.Zero, (Money)toRemoteOutput.Amount);
    }

    [Fact]
    public void Given_ToRemoteOutputs_When_ComparingThem_Then_UsesTransactionOutputComparer()
    {
        // Given
        var output1 = new ToRemoteOutput(_remotePubKey, new LightningMoney(1000000));
        var output2 = new ToRemoteOutput(_remotePubKey, new LightningMoney(2000000));

        // When
        var comparison = output1.CompareTo(output2);

        // Then
        Assert.NotEqual(0, comparison); // The actual comparison is handled by TransactionOutputComparer
    }

    [Fact]
    public void Given_DifferentPubKeys_When_ConstructingToRemoteOutputs_Then_GeneratesDifferentScripts()
    {
        // Given
        var remotePubKey2 = new PubKey("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");

        // When
        var output1 = new ToRemoteOutput(_remotePubKey, _amount);
        var output2 = new ToRemoteOutput(remotePubKey2, _amount);

        // Then
        Assert.NotEqual(output1.RedeemScript, output2.RedeemScript);
        Assert.NotEqual(output1.ScriptPubKey, output2.ScriptPubKey);
    }
}