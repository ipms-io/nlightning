using NLightning.Common.Managers;

namespace NLightning.Bolts.Tests.BOLT11;

using Bolts.BOLT11;
using Common.Constants;
using Common.Types;

public class InvoiceTests
{
    #region HumanReadablePart
    [Theory]
    [InlineData(NetworkConstants.MAINNET, 100_000_000_000, "lnbc1")]
    [InlineData(NetworkConstants.TESTNET, 100_000_000_000, "lntb1")]
    [InlineData(NetworkConstants.REGTEST, 100_000_000_000, "lnbcrt1")]
    [InlineData(NetworkConstants.SIGNET, 100_000_000_000, "lntbs1")]
    public void Given_NetworkType_When_InvoiceIsCreated_Then_PrefixIsCorrect(string network, ulong amountMsats, string expectedPrefix)
    {
        // Act
        var invoice = new Invoice(network, amountMsats);

        // Assert
        Assert.StartsWith(expectedPrefix, invoice.HumanReadablePart);
    }

    [Theory]
    [InlineData(NetworkConstants.MAINNET, 1, "lnbc10p")]
    [InlineData(NetworkConstants.MAINNET, 10, "lnbc100p")]
    [InlineData(NetworkConstants.MAINNET, 100, "lnbc1n")]
    [InlineData(NetworkConstants.MAINNET, 1_000, "lnbc10n")]
    [InlineData(NetworkConstants.MAINNET, 10_000, "lnbc100n")]
    [InlineData(NetworkConstants.MAINNET, 100_000, "lnbc1u")]
    [InlineData(NetworkConstants.MAINNET, 1_000_000, "lnbc10u")]
    [InlineData(NetworkConstants.MAINNET, 10_000_000, "lnbc100u")]
    [InlineData(NetworkConstants.MAINNET, 100_000_000, "lnbc1m")]
    [InlineData(NetworkConstants.MAINNET, 1_000_000_000, "lnbc10m")]
    [InlineData(NetworkConstants.MAINNET, 10_000_000_000, "lnbc100m")]
    [InlineData(NetworkConstants.MAINNET, 100_000_000_000, "lnbc1")]
    [InlineData(NetworkConstants.MAINNET, 1_000_000_000_000, "lnbc10")]
    [InlineData(NetworkConstants.MAINNET, 10_000_000_000_000, "lnbc100")]
    public void Given_Amount_When_InvoiceIsCreated_Then_AmountIsCorrect(string network, ulong amountMsats, string expectedHumanReadablePart)
    {
        // Act
        var invoice = new Invoice(network, amountMsats);

        // Assert
        Assert.Equal(expectedHumanReadablePart, invoice.HumanReadablePart);
    }

    [Fact]
    public void Given_ZeroAmount_When_InvoiceIsCreated_Then_HumanReadablePartJustContainPrefix()
    {
        // Arrange
        var network = Network.MAIN_NET;

        // Act
        var invoice = new Invoice(network);

        // Assert
        Assert.Equal("lnbc", invoice.HumanReadablePart);
    }

    [Fact]
    public void Given_InvalidNetwork_When_InvoiceIsCreated_Then_ExceptionIsThrown()
    {
        // Arrange
        var invalidNetwork = (Network)"invalid";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Invoice(invalidNetwork, 1000));
    }

    [Theory]
    [InlineData(NetworkConstants.MAINNET, 1, "lnbc10n")]
    [InlineData(NetworkConstants.MAINNET, 10, "lnbc100n")]
    [InlineData(NetworkConstants.MAINNET, 100, "lnbc1u")]
    [InlineData(NetworkConstants.MAINNET, 1_000, "lnbc10u")]
    [InlineData(NetworkConstants.MAINNET, 10_000, "lnbc100u")]
    [InlineData(NetworkConstants.MAINNET, 100_000, "lnbc1m")]
    [InlineData(NetworkConstants.MAINNET, 1_000_000, "lnbc10m")]
    [InlineData(NetworkConstants.MAINNET, 10_000_000, "lnbc100m")]
    [InlineData(NetworkConstants.MAINNET, 100_000_000, "lnbc1")]
    [InlineData(NetworkConstants.MAINNET, 1_000_000_000, "lnbc10")]
    [InlineData(NetworkConstants.MAINNET, 10_000_000_000, "lnbc100")]
    public void Given_Amount_When_InvoiceIsCreatedWithInSatoshis_Then_AmountIsCorrect(string network, long amountSats, string expectedHumanReadablePart)
    {
        // Arrange
        ConfigManager.Instance.Network = network;

        // Act
        var invoice = Invoice.InSatoshis(amountSats);

        // Assert
        Assert.Equal(expectedHumanReadablePart, invoice.HumanReadablePart);
    }
    #endregion
}