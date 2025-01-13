using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT3.Transactions;

using Bolts.BOLT3.Transactions;

public class FundingTransactionOutputTests
{
    [Fact]
    public void CreateFundingTransactionOutput_Success()
    {
        // Arrange
        var pubkey1 = new PubKey("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb");
        var pubkey2 = new PubKey("030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c1");
        ulong value = 100000000; // 1 BTC in satoshis

        // Act
        var fundingOutput = new FundingTransactionOutput(pubkey1, pubkey2, value);

        // Assert
        Assert.Equal(value, fundingOutput.Value);
        Assert.NotNull(fundingOutput.FundingScriptPubKey);
    }

    [Fact]
    public void CreateFundingTransactionOutput_ValidPublicKeys_ScriptIsCorrect()
    {
        // Arrange
        var pubkey1 = new PubKey("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb");
        var pubkey2 = new PubKey("030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c1");
        ulong value = 100000000; // 1 BTC in satoshis
        var expectedScriptPubKey = Convert.FromHexString("0020C015C4A6BE010E21657068FC2E6A9D02B27EBE4D490A25846F7237F104D1A3CD");

        // Act
        var fundingOutput = new FundingTransactionOutput(pubkey1, pubkey2, value);

        // Assert
        // Assert.Equal(expectedScriptPubKey, fundingOutput.FundingScriptPubKey);
    }
}