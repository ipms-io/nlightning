using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Tests.Outputs;

using Bitcoin.Outputs;
using Domain.Money;

public class ChangeOutputTests
{
    private readonly Script _redeemScript = Script.FromHex("21034F355BDCB7CC0AF728EF3CCEB9615D90684BB5B2CA5F859AB0F0B704075871AAAD51B2");
    private readonly Script _scriptPubKey = Script.FromHex("002032E8DA66B7054D40832C6A7A66DF79D8D7BCCCD5FFA53F5DD1772CB9CB9F3283");
    private readonly LightningMoney _amount = new(1000000);

    [Fact]
    public void Given_ScriptPubKeyAndAmount_When_ConstructingChangeOutput_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var changeOutput = new ChangeOutput(_scriptPubKey, _amount);

        // Then
        Assert.Equal(_scriptPubKey, changeOutput.ScriptPubKey);
        Assert.Equal(_scriptPubKey, changeOutput.RedeemScript);
        Assert.Equal(_amount, changeOutput.Amount);
        Assert.Equal(ScriptType.P2WPKH, changeOutput.ScriptType);
    }

    [Fact]
    public void Given_RedeemScriptScriptPubKeyAndAmount_When_ConstructingChangeOutput_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var changeOutput = new ChangeOutput(_redeemScript, _scriptPubKey, _amount);

        // Then
        Assert.Equal(_redeemScript, changeOutput.RedeemScript);
        Assert.Equal(_scriptPubKey, changeOutput.ScriptPubKey);
        Assert.Equal(_amount, changeOutput.Amount);
        Assert.Equal(ScriptType.P2WPKH, changeOutput.ScriptType);
    }

    [Fact]
    public void Given_ScriptPubKeyAndNullAmount_When_ConstructingChangeOutput_Then_AmountIsZero()
    {
        // Given
        LightningMoney? amount = null;

        // When
        var changeOutput = new ChangeOutput(_scriptPubKey, amount);

        // Then
        Assert.Equal(_scriptPubKey, changeOutput.ScriptPubKey);
        Assert.Equal(new LightningMoney(0), changeOutput.Amount);
        Assert.Equal(ScriptType.P2WPKH, changeOutput.ScriptType);
    }

    [Fact]
    public void Given_RedeemScriptScriptPubKeyAndNullAmount_When_ConstructingChangeOutput_Then_AmountIsZero()
    {
        // Given
        LightningMoney? amount = null;

        // When
        var changeOutput = new ChangeOutput(_redeemScript, _scriptPubKey, amount);

        // Then
        Assert.Equal(_redeemScript, changeOutput.RedeemScript);
        Assert.Equal(_scriptPubKey, changeOutput.ScriptPubKey);
        Assert.Equal(new LightningMoney(0), changeOutput.Amount);
        Assert.Equal(ScriptType.P2WPKH, changeOutput.ScriptType);
    }

    [Fact]
    public void Given_ChangeOutputWithZeroAmount_When_ToCoinCalled_Then_ThrowsInvalidOperationException()
    {
        // Given
        var changeOutput = new ChangeOutput(_scriptPubKey)
        {
            TxId = uint256.Parse("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"),
            Index = 1
        };

        // When/Then
        Assert.Throws<InvalidOperationException>(() => changeOutput.ToCoin());
    }

    [Fact]
    public void Given_ChangeOutputWithValidAmount_When_ToCoinCalled_Then_ReturnsCorrectCoin()
    {
        // Given
        var changeOutput = new ChangeOutput(_redeemScript, _amount)
        {
            TxId = uint256.Parse("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"),
            Index = 1
        };

        // When
        var coin = changeOutput.ToCoin();

        // Then
        Assert.Equal(changeOutput.TxId, coin.Outpoint.Hash);
        Assert.Equal(changeOutput.Index, (int)coin.Outpoint.N);
        Assert.Equal((Money)changeOutput.Amount, coin.Amount);
        Assert.Equal(changeOutput.ScriptPubKey, coin.ScriptPubKey);
    }

    [Fact]
    public void Given_TwoChangeOutputs_When_ComparingThem_Then_UsesTransactionOutputComparer()
    {
        // Given
        var output1 = new ChangeOutput(_scriptPubKey, new LightningMoney(1000000));
        var output2 = new ChangeOutput(_scriptPubKey, new LightningMoney(2000000));

        // When
        var comparison = output1.CompareTo(output2);

        // Then
        Assert.NotEqual(0, comparison); // The actual comparison is handled by TransactionOutputComparer
    }
}