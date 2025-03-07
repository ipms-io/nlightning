using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT11;

using BOLT8.Utils;
using Bolts.BOLT11;
using Bolts.BOLT11.Constants;
using Bolts.BOLT11.Types;
using Common.Constants;
using Common.Managers;
using Common.Types;
using Exceptions;

[Collection("secure-key-manager")]
public class InvoiceTests
{
    private static readonly uint256 s_testPaymentHash = new("0001020304050607080900010203040506070809000102030405060708090102");
    private static readonly uint256 s_testPaymentSecret = new("1111111111111111111111111111111111111111111111111111111111111111");
    private static readonly RoutingInfo s_defaultRoutingInfo = new(
        new PubKey(InitiatorValidKeysUtil.RemoteStaticPublicKey),
        new ShortChannelId(870127, 1237, 1), 1, 1, 1
    );

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
    public void Given_Amount_When_InvoiceIsCreatedWithInSatoshis_Then_AmountIsCorrect(string network, ulong amountSats, string expectedHumanReadablePart)
    {
        // Arrange
        ConfigManager.Instance.Network = network;

        // Act
        var invoice = Invoice.InSatoshis(amountSats, "test", s_testPaymentHash, s_testPaymentSecret);

        // Assert
        Assert.Equal(expectedHumanReadablePart, invoice.HumanReadablePart);
    }
    #endregion

    #region Tagged Fields
    [Fact]
    public void Given_Invoice_When_SetTaggedField_Then_InvoiceStringClearedOnChange()
    {
        // Given
        var invoice = new Invoice(Network.MAIN_NET);

        // "Touch" the invoice string once, so it's cached
        var initialStr = invoice.ToString();
        Assert.False(string.IsNullOrEmpty(initialStr));

        // When
        invoice.RoutingInfos = new RoutingInfoCollection { s_defaultRoutingInfo };

        // Then
        // The .Changed event on routingInfoCollection should have reset invoice's cached string to null,
        // forcing a new encode on next invoice.ToString().
        var reencodedStr = invoice.ToString();
        Assert.NotEqual(initialStr, reencodedStr);
        Assert.NotNull(invoice.RoutingInfos);
        Assert.Single(invoice.RoutingInfos!);
        Assert.Equal(s_defaultRoutingInfo.PubKey, invoice.RoutingInfos![0].PubKey);
    }

    [Fact]
    public void Given_Invoice_When_SetFallbackAddresses_Then_TheyAreStoredAndRetrieved()
    {
        // Given
        var invoice = new Invoice(Network.MAIN_NET);
        var addresses = new List<BitcoinAddress>
        {
            BitcoinAddress.Create("3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX", Network.MAIN_NET),
            BitcoinAddress.Create("1RustyRX2oai4EYYDpQGWvEL62BBGqN9T", Network.MAIN_NET)
        };

        // When
        invoice.FallbackAddresses = addresses;

        // Then
        Assert.NotNull(invoice.FallbackAddresses);
        Assert.Equal(2, invoice.FallbackAddresses!.Count);
        Assert.Contains(addresses[0], invoice.FallbackAddresses);
        Assert.Contains(addresses[1], invoice.FallbackAddresses);
    }

    [Fact]
    public void Given_Invoice_When_AddRoutingInfo_Then_InvoiceStringClearedOnInternalChange()
    {
        // Given
        var invoice = new Invoice(Network.MAIN_NET)
        {
            RoutingInfos = new RoutingInfoCollection { s_defaultRoutingInfo }
        };

        // "Touch" the invoice string once so it's cached
        var initialStr = invoice.ToString();
        Assert.False(string.IsNullOrEmpty(initialStr));

        // When
        invoice.RoutingInfos.Add(s_defaultRoutingInfo);

        // Then
        // The .Changed event on routingInfoCollection should have reset invoice's cached string to null,
        // forcing a new encode on next invoice.ToString().
        var reencodedStr = invoice.ToString();
        Assert.NotEqual(initialStr, reencodedStr);
        Assert.NotNull(invoice.RoutingInfos);
        Assert.Equal(2, invoice.RoutingInfos!.Count);
    }

    [Fact]
    public void Given_Invoice_When_SetExpiryDate_Then_TaggedFieldIsCreatedAndExpiryDateComputedCorrectly()
    {
        // Given
        var now = DateTimeOffset.UtcNow;
        var invoice = new Invoice(Network.MAIN_NET, 0, now.ToUnixTimeSeconds());

        // By default, if no expiry tag is set, ExpiryDate = timestamp + DEFAULT_EXPIRATION_SECONDS
        var defaultExpiry = now.AddSeconds(InvoiceConstants.DEFAULT_EXPIRATION_SECONDS);
        Assert.Equal(defaultExpiry.ToUnixTimeSeconds(), invoice.ExpiryDate.ToUnixTimeSeconds());

        // When
        var customExpiry = now.AddHours(2);
        invoice.ExpiryDate = customExpiry; // => sets a new ExpiryTimeTaggedField

        // Then
        // The difference: customExpiry.ToUnixTimeSeconds() - now.ToUnixTimeSeconds() => stored in the field
        var newComputedExpiry = invoice.ExpiryDate;
        Assert.Equal(customExpiry.ToUnixTimeSeconds(), newComputedExpiry.ToUnixTimeSeconds());
    }
    #endregion

    #region Encoding/Decoding
    [Fact]
    public void Given_NewInvoice_When_EncodeCalled_Then_ReturnsNonEmptyString()
    {
        // Given
        ConfigManager.Instance.Network = Network.MAIN_NET;
        var invoice = new Invoice(1000, "TestDesc", uint256.One, uint256.Zero)
        {
            PayeePubKey = new PubKey("020202020202020202020202020202020202020202020202020202020202020202")
        };

        // When
        var encoded = invoice.Encode();

        // Then
        Assert.False(string.IsNullOrWhiteSpace(encoded));
    }

    [Fact]
    public void Given_EmptyString_When_DecodeCalled_Then_InvoiceSerializationExceptionIsThrown()
    {
        // Given
        var invalidInvoice = string.Empty;

        // When / Then
        var ex = Assert.Throws<InvoiceSerializationException>(() => Invoice.Decode(invalidInvoice));
        Assert.Contains("Invoice string was empty", ex.Message);
    }

    [Fact]
    public void Given_InvalidInvoiceString_When_DecodeCalled_Then_InvoiceSerializationExceptionIsThrown()
    {
        // Given
        var invalidInvoice = "lnxyaaaaaa";

        // When / Then
        Assert.Throws<InvoiceSerializationException>(() => Invoice.Decode(invalidInvoice));
    }

    [Fact]
    public void Given_ValidInvoiceString_When_DecodeCalled_Then_InvoiceIsReturned()
    {
        // Given
        const string INVOICE_STRING = "lnbc20m1pvjluezsp5zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zygshp58yjmdan79s6qqdhdzgynm4zwqd5d7xmw5fk98klysy043l2ahrqspp5qqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqypqfppj3a24vwu6r8ejrss3axul8rxldph2q7z99qrsgqz6qsgww34xlatfj6e3sngrwfy3ytkt29d2qttr8qz2mnedfqysuqypgqex4haa2h8fx3wnypranf3pdwyluftwe680jjcfp438u82xqphf75ym";
        ConfigManager.Instance.Network = Network.MAIN_NET;

        // When
        var invoice = Invoice.Decode(INVOICE_STRING);

        // Then
        Assert.Equal(2000000000U, invoice.AmountMilliSats);
    }
    #endregion
}