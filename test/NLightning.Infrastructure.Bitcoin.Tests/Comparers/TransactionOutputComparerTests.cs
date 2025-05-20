using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Tests.Comparers;

using Bitcoin.Comparers;
using Bitcoin.Outputs;
using Domain.Money;

public class TransactionOutputComparerTests
{
    private readonly Script _script1 = Script.FromHex("0014a1b2c3");
    private readonly Script _script2 = Script.FromHex("0014a1b2c4");
    private readonly Script _script3 = Script.FromHex("0014a1b2c3d4");
    private readonly PubKey _pubKey = new("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa");
    private readonly PubKey _revocationPubKey = new("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991");
    private readonly LightningMoney _anchorAmount = LightningMoney.Satoshis(330);

    [Fact]
    public void Given_NullOutputs_When_Comparing_Then_HandlesNullsCorrectly()
    {
        // Given
        BaseOutput? output1 = null;
        BaseOutput? output2 = null;
        var changeOutput = new ChangeOutput(_script1);
        var comparer = new TransactionOutputComparer();

        // When/Then
        Assert.Equal(0, comparer.Compare(output1, output2)); // null, null
        Assert.Equal(-1, comparer.Compare(output1, changeOutput)); // null, not null
        Assert.Equal(1, comparer.Compare(changeOutput, output1)); // not null, null
    }

    [Fact]
    public void Given_OutputsWithDifferentAmounts_When_Comparing_Then_SortsByAmount()
    {
        // Given
        var output1 = new ChangeOutput(_script1, new LightningMoney(1000));
        var output2 = new ChangeOutput(_script2, new LightningMoney(2000));
        var output3 = new ChangeOutput(_script3, new LightningMoney(3000));
        var outputs = new List<BaseOutput> { output3, output1, output2 };
        var comparer = TransactionOutputComparer.Instance;

        // When
        outputs.Sort(comparer);

        // Then
        Assert.Equal(output1, outputs[0]);
        Assert.Equal(output2, outputs[1]);
        Assert.Equal(output3, outputs[2]);
    }

    [Fact]
    public void Given_OutputsWithSameAmountButDifferentScripts_When_Comparing_Then_SortsByScriptPubKeyLexicographically()
    {
        // Given
        var output1 = new ChangeOutput(_script1, new LightningMoney(1000));
        var output2 = new ChangeOutput(_script2, new LightningMoney(1000));
        var output3 = new ChangeOutput(_script3, new LightningMoney(1000));
        var outputs = new List<BaseOutput> { output2, output3, output1 };
        var comparer = TransactionOutputComparer.Instance;

        // When
        outputs.Sort(comparer);

        // Then
        Assert.Equal(output1, outputs[0]); // 0014a1b2c3
        Assert.Equal(output3, outputs[1]); // 0014a1b2c3d4 (starts with same bytes as output1)
        Assert.Equal(output2, outputs[2]); // 0014a1b2c4
    }

    [Fact]
    public void Given_OutputsWithSameScriptPrefixButDifferentLengths_When_Comparing_Then_SortsByLength()
    {
        // Given
        var script1 = Script.FromHex("0014a1");
        var script2 = Script.FromHex("0014a1b2");
        var script3 = Script.FromHex("0014a1b2c3");
        var output1 = new ChangeOutput(script1, new LightningMoney(1000));
        var output2 = new ChangeOutput(script2, new LightningMoney(1000));
        var output3 = new ChangeOutput(script3, new LightningMoney(1000));
        var outputs = new List<BaseOutput> { output3, output2, output1 };
        var comparer = TransactionOutputComparer.Instance;

        // When
        outputs.Sort(comparer);

        // Then
        Assert.Equal(output1, outputs[0]);
        Assert.Equal(output2, outputs[1]);
        Assert.Equal(output3, outputs[2]);
    }

    [Fact]
    public void Given_HtlcOutputsWithSameAmountAndScript_When_Comparing_Then_SortsByCltvExpiry()
    {
        // Given
        var htlc1 = new OfferedHtlcOutput(_anchorAmount, _revocationPubKey, _pubKey, _pubKey,
                                          new ReadOnlyMemory<byte>([0]), new LightningMoney(1000), 1);
        var htlc2 = new OfferedHtlcOutput(_anchorAmount, _revocationPubKey, _pubKey, _pubKey,
                                          new ReadOnlyMemory<byte>([0]), new LightningMoney(1000), 2);
        var htlc3 = new OfferedHtlcOutput(_anchorAmount, _revocationPubKey, _pubKey, _pubKey,
                                          new ReadOnlyMemory<byte>([0]), new LightningMoney(1000), 3);
        var outputs = new List<BaseOutput> { htlc3, htlc2, htlc1 };
        var comparer = TransactionOutputComparer.Instance;

        // When
        outputs.Sort(comparer);

        // Then
        Assert.Equal(htlc1, outputs[0]);
        Assert.Equal(htlc2, outputs[1]);
        Assert.Equal(htlc3, outputs[2]);
    }

    [Fact]
    public void Given_MixedOutputTypes_When_Comparing_Then_SortsByAmountThenScript()
    {
        // Given
        var change1 = new ChangeOutput(_script1, new LightningMoney(1000));
        var htlc1 = new OfferedHtlcOutput(_anchorAmount, _revocationPubKey, _pubKey, _pubKey,
                                          new ReadOnlyMemory<byte>([0]), new LightningMoney(1000), 500);
        var toRemote = new ToRemoteOutput(true, _pubKey, new LightningMoney(2000));
        var toLocal = new ToLocalOutput(_pubKey, _revocationPubKey, 144, new LightningMoney(3000));
        var outputs = new List<BaseOutput> { toLocal, toRemote, htlc1, change1 };
        var comparer = TransactionOutputComparer.Instance;

        // When
        outputs.Sort(comparer);

        // Then
        // First sort by amount
        Assert.Equal(change1.Amount, htlc1.Amount);
        Assert.True(outputs.IndexOf(change1) < outputs.IndexOf(toRemote));
        Assert.True(outputs.IndexOf(toRemote) < outputs.IndexOf(toLocal));

        // change1 and htlc1 have same amount, so sort by script
        var compareScripts = string.Compare(
            change1.ScriptPubKey.ToHex(),
            htlc1.ScriptPubKey.ToHex(),
            StringComparison.Ordinal);

        if (compareScripts < 0)
            Assert.True(outputs.IndexOf(change1) < outputs.IndexOf(htlc1));
        else
            Assert.True(outputs.IndexOf(htlc1) < outputs.IndexOf(change1));
    }

    [Fact]
    public void Given_SingletonInstance_When_Accessed_Then_ReturnsSameInstance()
    {
        // Given
        var instance1 = TransactionOutputComparer.Instance;
        var instance2 = TransactionOutputComparer.Instance;

        // When/Then
        Assert.Same(instance1, instance2);
    }
}