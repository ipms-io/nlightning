using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Outputs;

using Bolts.BOLT3.Outputs;
using Common.Types;

public class BaseOutputTests
{
    // Concrete implementation for testing the abstract class
    private class FakeOutput : BaseOutput
    {
        public FakeOutput(Script redeemScript, Script scriptPubKey, LightningMoney amountMilliSats)
            : base(redeemScript, scriptPubKey, amountMilliSats)
        {
        }

        public FakeOutput(Script redeemScript, LightningMoney amountMilliSats)
            : base(redeemScript, amountMilliSats)
        {
        }

        public override ScriptType ScriptType => ScriptType.P2WPKH;
    }

    private readonly Script _redeemScript = Script.FromHex("21034F355BDCB7CC0AF728EF3CCEB9615D90684BB5B2CA5F859AB0F0B704075871AAAD51B2");
    private readonly Script _scriptPubKey = Script.FromHex("002032E8DA66B7054D40832C6A7A66DF79D8D7BCCCD5FFA53F5DD1772CB9CB9F3283");
    private readonly LightningMoney _amount = new(1000000);

    [Fact]
    public void Given_ValidParameters_When_ConstructingBaseOutputWithScriptPubKey_Then_PropertiesAreSetCorrectly()
    {
        // Given

        // When
        var output = new FakeOutput(_redeemScript, _scriptPubKey, _amount);

        // Then
        Assert.Equal(_redeemScript, output.RedeemScript);
        Assert.Equal(_scriptPubKey, output.ScriptPubKey);
        Assert.Equal(_amount, output.AmountMilliSats);
        Assert.Equal(ScriptType.P2WPKH, output.ScriptType);
        Assert.Null(output.TxId);
        Assert.Equal(0u, output.Index);
    }

    [Fact]
    public void Given_ValidParameters_When_ConstructingBaseOutputWithoutScriptPubKey_Then_ScriptPubKeyIsDerived()
    {
        // Given

        // When
        var output = new FakeOutput(_redeemScript, _amount);

        // Then
        Assert.Equal(_redeemScript, output.RedeemScript);
        Assert.Equal(_scriptPubKey, output.ScriptPubKey);
        Assert.Equal(_amount, output.AmountMilliSats);
    }

    [Fact]
    public void Given_BaseOutput_When_ToTxOutCalled_Then_ReturnsCorrectTxOut()
    {
        // Given
        var output = new FakeOutput(_redeemScript, _scriptPubKey, _amount);

        // When
        var txOut = output.ToTxOut();

        // Then
        Assert.Equal((Money)_amount, txOut.Value);
        Assert.Equal(_scriptPubKey, txOut.ScriptPubKey);
    }

    [Fact]
    public void Given_BaseOutputWithValidTxId_When_ToCoinCalled_Then_ReturnsCorrectCoin()
    {
        // Given
        var output = new FakeOutput(_redeemScript, _scriptPubKey, _amount)
        {
            TxId = uint256.Parse("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be"),
            Index = 1
        };

        // When
        var coin = output.ToCoin();

        // Then
        Assert.Equal(output.TxId, coin.Outpoint.Hash);
        Assert.Equal(output.Index, coin.Outpoint.N);
        Assert.Equal((Money)output.AmountMilliSats, coin.Amount);
        Assert.Equal(output.ScriptPubKey, coin.ScriptPubKey);
        Assert.Equal(output.RedeemScript, coin.Redeem);
    }

    [Fact]
    public void Given_BaseOutputWithoutTxId_When_ToCoinCalled_Then_ThrowsInvalidOperationException()
    {
        // Given
        var output = new FakeOutput(_redeemScript, _scriptPubKey, _amount);

        // When/Then
        Assert.Throws<InvalidOperationException>(() => output.ToCoin());

        output.TxId = uint256.Zero;
        Assert.Throws<InvalidOperationException>(() => output.ToCoin());

        output.TxId = uint256.One;
        Assert.Throws<InvalidOperationException>(() => output.ToCoin());
    }

    [Fact]
    public void Given_BaseOutputWithZeroAmount_When_ToCoinCalled_Then_ThrowsInvalidOperationException()
    {
        // Given
        var output = new FakeOutput(_redeemScript, _scriptPubKey, new LightningMoney(0))
        {
            TxId = uint256.Parse("8984484a580b825b9972d7adb15050b3ab624ccd731946b3eeddb92f4e7ef6be")
        };

        // When/Then
        Assert.Throws<InvalidOperationException>(() => output.ToCoin());
    }

    [Fact]
    public void Given_TwoOutputs_When_CompareToIsCalled_Then_UsesTransactionOutputComparer()
    {
        // Given
        var output1 = new FakeOutput(_redeemScript, _scriptPubKey, new LightningMoney(1000000));
        var output2 = new FakeOutput(_redeemScript, _scriptPubKey, new LightningMoney(2000000));

        // When
        var comparison = output1.CompareTo(output2);

        // Then
        Assert.NotEqual(0, comparison); // Actual comparison logic is in the TransactionOutputComparer

        // Also check null comparison
        Assert.Equal(1, output1.CompareTo(null));
    }
}