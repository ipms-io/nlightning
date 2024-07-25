namespace NLightning.Bolts.Tests.BOLT11;

using Bolts.BOLT11;
using Common;
using Common.Constants;

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
    public void Given_ZeroAmount_When_InvoiceIsCreated_Then_HumanReadablePartContains1p()
    {
        // Arrange
        var network = Network.MainNet;
        ulong amountMsats = 0;

        // Act
        var invoice = new Invoice(network, amountMsats);

        // Assert
        Assert.Equal("lnbc1p", invoice.HumanReadablePart);
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
    public void Given_Amount_When_InvoiceIsCreatedWithInSatoshis_Then_AmountIsCorrect(string network, ulong amountSats, string expectedHumanReadablePart)
    {
        // Act
        var invoice = Invoice.InSatoshis(network, amountSats);

        // Assert
        Assert.Equal(expectedHumanReadablePart, invoice.HumanReadablePart);
    }
    #endregion

    #region Parse
    [Theory]
    [InlineData("lnbc1pvjluezsp5zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zygspp5qqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqypqdpl2pkx2ctnv5sxxmmwwd5kgetjypeh2ursdae8g6twvus8g6rfwvs8qun0dfjkxaq9qrsgq357wnc5r2ueh7ck6q93dj32dlqnls087fxdwk8qakdyafkq3yap9us6v52vjjsrvywa6rt52cm9r9zqt8r2t7mlcwspyetp5h2tztugp9lfyql", NetworkConstants.MAINNET, 0)]
    [InlineData("lnbc20m1pvjluezsp5zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zygspp5qqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqypqhp58yjmdan79s6qqdhdzgynm4zwqd5d7xmw5fk98klysy043l2ahrqsfpp3qjmp7lwpagxun9pygexvgpjdc4jdj85fr9yq20q82gphp2nflc7jtzrcazrra7wwgzxqc8u7754cdlpfrmccae92qgzqvzq2ps8pqqqqqqpqqqqq9qqqvpeuqafqxu92d8lr6fvg0r5gv0heeeqgcrqlnm6jhphu9y00rrhy4grqszsvpcgpy9qqqqqqgqqqqq7qqzq9qrsgqdfjcdk6w3ak5pca9hwfwfh63zrrz06wwfya0ydlzpgzxkn5xagsqz7x9j4jwe7yj7vaf2k9lqsdk45kts2fd0fkr28am0u4w95tt2nsq76cqw0", NetworkConstants.MAINNET, 2000000000)]
    [InlineData("lnbc27070n1pn2z35npp56qhmnjr8kj5ufrjna5z7vssj2w656n08hyu2mx9h4fujclagja8qdqqcqzzgxqyz5vqrzjqwnvuc0u4txn35cafc7w94gxvq5p3cu9dd95f7hlrh0fvs46wpvhdygzdvy829w5ryqqqqryqqqqthqqpysp5hn54f6556pn9yjvakannw32qq797vqnzsgt2h05ptrkhy7dj72ns9qrsgqupw0qf4n36jtqxux0992ltmux3qrlvhfmc23xgmgpp854nzejjgnaeekctp8dmh4682alrythcuv72pdahxgevfhdw7wvu9u5zugelspz25zl7", NetworkConstants.MAINNET, 2707000)]
    public void Given_ValidInvoiceString_When_Parsing_Then_InvoiceIsCorrect(string invoiceString, string expectedNetwork, ulong expectedAmount)
    {
        // Act
        var invoice = Invoice.Parse(invoiceString);

        // Assert
        Assert.Equal(expectedNetwork, invoice.Network);
        Assert.Equal(expectedAmount, invoice.AmountMsats);
    }
    #endregion
}