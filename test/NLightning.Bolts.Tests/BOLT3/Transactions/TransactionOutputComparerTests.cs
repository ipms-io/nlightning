namespace NLightning.Bolts.Tests.BOLT3.Transactions;

using Bolts.BOLT3.Transactions;

public class TransactionOutputComparerTests
{
    [Fact]
    public void Compare_SortByValue()
    {
        // Arrange
        var output1 = new TransactionOutput(2000, [0x01, 0x03], 102);
        var output2 = new TransactionOutput(3000, [0x01, 0x01], 103);
        var output3 = new TransactionOutput(5000, [0x01, 0x02], 105);

        var outputs = new List<TransactionOutput> { output3, output2, output1 };

        // Act
        outputs.Sort(new TransactionOutputComparer());

        // Assert
        Assert.Equal(output1, outputs[0]);
        Assert.Equal(output2, outputs[1]);
        Assert.Equal(output3, outputs[2]);
    }

    [Fact]
    public void Compare_SortByScriptPubKey()
    {
        // Arrange
        var output1 = new TransactionOutput(3000, [0x01, 0x01], 101);
        var output2 = new TransactionOutput(3000, [0x01, 0x02], 102);
        var output3 = new TransactionOutput(3000, [0x01, 0x03], 103);

        var outputs = new List<TransactionOutput> { output3, output2, output1 };

        // Act
        outputs.Sort(new TransactionOutputComparer());

        // Assert
        Assert.Equal(output1, outputs[0]);
        Assert.Equal(output2, outputs[1]);
        Assert.Equal(output3, outputs[2]);
    }

    [Fact]
    public void Compare_SortByCltvExpiry()
    {
        // Arrange
        var output1 = new TransactionOutput(5000, [0x01, 0x02], 100);
        var output2 = new TransactionOutput(5000, [0x01, 0x02], 101);
        var output3 = new TransactionOutput(5000, [0x01, 0x02], 102);

        var outputs = new List<TransactionOutput> { output2, output3, output1 };

        // Act
        outputs.Sort(new TransactionOutputComparer());

        // Assert
        Assert.Equal(output1, outputs[0]); // No CLTV expiry
        Assert.Equal(output2, outputs[1]);
        Assert.Equal(output3, outputs[2]);
    }

    [Fact]
    public void Compare_SortByScriptPubKeyLength()
    {
        // Arrange
        var output1 = new TransactionOutput(3000, [0x01], 101);
        var output2 = new TransactionOutput(3000, [0x01, 0x02], 102);
        var output3 = new TransactionOutput(3000, [0x01, 0x02, 0x03], 103);

        var outputs = new List<TransactionOutput> { output3, output2, output1 };

        // Act
        outputs.Sort(new TransactionOutputComparer());

        // Assert
        Assert.Equal(output1, outputs[0]);
        Assert.Equal(output2, outputs[1]);
        Assert.Equal(output3, outputs[2]);
    }
}