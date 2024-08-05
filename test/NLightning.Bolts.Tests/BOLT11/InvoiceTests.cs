using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT11;

using System.Text;
using Bolts.BOLT11;
using Bolts.BOLT11.Types;
using Bolts.BOLT9;
using Common;
using Common.Constants;
using NLightning.Bolts.BOLT8.Constants;
using NLightning.Bolts.BOLT8.Hashes;
using NLightning.Common.Types;
// using Common.Utils;
using static Utils.TestUtils;

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
        var network = Network.MainNet;
        ulong amountMsats = 0;

        // Act
        var invoice = new Invoice(network, amountMsats);

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
        // Act
        var invoice = Invoice.InSatoshis(network, amountSats);

        // Assert
        Assert.Equal(expectedHumanReadablePart, invoice.HumanReadablePart);
    }
    #endregion

    #region Parse
    [Fact]
    public void Given_ValidInvoiceString_When_Parsing_Then_InvoiceIsCorrect()
    {
        // Arrange
        var testInvoices = ReadTestInvoices("BOLT11/Invoices/ValidInvoices.txt");

        // Act
        foreach (var testInvoice in testInvoices)
        {
            var invoice = Invoice.Decode(testInvoice.InvoiceString);

            // Assert
            Assert.Equal(testInvoice.ExpectedNetwork, invoice.Network);
            Assert.Equal(testInvoice.ExpectedAmountMilliSats, invoice.AmountMsats);
            Assert.Equal(testInvoice.ExpectedTimestamp, invoice.Timestamp);
            Assert.Equal(testInvoice.ExpectedPaymentHash, invoice.PaymentHash);
            Assert.Equal(testInvoice.ExpectedPaymentSecret, invoice.PaymentSecret);
            Assert.Equal(testInvoice.ExpectedDescription, invoice.Description);
            if (testInvoice.ExpectedExpiryTime != null)
            {
                Assert.Equal(testInvoice.ExpectedExpiryTime, invoice.ExpiryDate);
            }
            Assert.Equal(testInvoice.ExpectedDescriptionHash, invoice.DescriptionHash);
            Assert.Equal(testInvoice.ExpectedFallbackAddress, invoice.FallbackAddresses?.FirstOrDefault());
            if (testInvoice.ExpectedFeatures != null)
            {
                Assert.NotNull(invoice.Features);
                Assert.True(testInvoice.ExpectedFeatures.IsCompatible(invoice.Features));
            }
            if (testInvoice.ExpectedRoutingInfo != null)
            {
                Assert.NotNull(invoice.RoutingInfos);
                Assert.Equal(testInvoice.ExpectedRoutingInfo.Count, invoice.RoutingInfos.Count);

                for (var i = 0; i < testInvoice.ExpectedRoutingInfo.Count; i++)
                {
                    Assert.Equal(testInvoice.ExpectedRoutingInfo[i].PubKey, invoice.RoutingInfos[i].PubKey);
                    Assert.Equal(testInvoice.ExpectedRoutingInfo[i].ShortChannelId, invoice.RoutingInfos[i].ShortChannelId);
                    Assert.Equal(testInvoice.ExpectedRoutingInfo[i].FeeBaseMsat, invoice.RoutingInfos[i].FeeBaseMsat);
                    Assert.Equal(testInvoice.ExpectedRoutingInfo[i].FeeProportionalMillionths, invoice.RoutingInfos[i].FeeProportionalMillionths);
                    Assert.Equal(testInvoice.ExpectedRoutingInfo[i].CltvExpiryDelta, invoice.RoutingInfos[i].CltvExpiryDelta);
                }
            }
            Assert.Equal(testInvoice.ExpectedMinFinalCltvExpiry, invoice.MinFinalCltvExpiry);
            Assert.Equal(testInvoice.ExpectedMetadata, invoice.Metadata);
        }
    }
    #endregion

    private class TestInvoice(string invoiceString)
    {
        public string InvoiceString = invoiceString;
        public Network? ExpectedNetwork;
        public ulong? ExpectedAmountMilliSats;
        public long? ExpectedTimestamp;
        public uint256? ExpectedPaymentHash;
        public uint256? ExpectedPaymentSecret;
        public Features? ExpectedFeatures;
        public RoutingInfoCollection? ExpectedRoutingInfo;
        public DateTimeOffset? ExpectedExpiryTime;
        public BitcoinAddress? ExpectedFallbackAddress;
        public string? ExpectedDescription;
        public PubKey? ExpectedPayeePubKey;
        public uint256? ExpectedDescriptionHash;
        public int? ExpectedMinFinalCltvExpiry;
        public byte[]? ExpectedMetadata;
    }

    private static List<TestInvoice> ReadTestInvoices(string filePath)
    {
        var testInvoice = new List<TestInvoice>();
        TestInvoice? currentInvoice = null;
        var hasher = new SHA256();

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("invoice="))
            {
                currentInvoice = new TestInvoice(line[8..]);
            }
            else if (line.StartsWith("network="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("network line without invoice line");
                }

                currentInvoice.ExpectedNetwork = new Network(line[8..]);
            }
            else if (line.StartsWith("amount="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("amount line without invoice line");
                }

                currentInvoice.ExpectedAmountMilliSats = ulong.Parse(line[7..]);
            }
            else if (line.StartsWith("timestamp="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("timestamp line without invoice line");
                }

                currentInvoice.ExpectedTimestamp = long.Parse(line[10..]);
            }
            else if (line.StartsWith("p="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("p line without invoice line");
                }

                currentInvoice.ExpectedPaymentHash = new uint256(GetBytes(line[2..]));
            }
            else if (line.StartsWith("s="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("s line without invoice line");
                }

                currentInvoice.ExpectedPaymentSecret = new uint256(GetBytes(line[2..]));
            }
            else if (line.StartsWith("d="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("d line without invoice line");
                }

                currentInvoice.ExpectedDescription = line[2..];
            }
            else if (line.StartsWith("x="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("x line without invoice line");
                }

                currentInvoice.ExpectedExpiryTime = DateTimeOffset.FromUnixTimeSeconds(currentInvoice.ExpectedTimestamp!.Value + long.Parse(line[2..]));
            }
            else if (line.StartsWith("h="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("h line without invoice line");
                }

                var hash = new byte[HashConstants.HASH_LEN];
                hasher.AppendData(Encoding.UTF8.GetBytes(line[2..]));
                hasher.GetHashAndReset(hash);
                currentInvoice.ExpectedDescriptionHash = new uint256(hash);
            }
            else if (line.StartsWith("f="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("f line without invoice line");
                }

                currentInvoice.ExpectedFallbackAddress = BitcoinAddress.Create(line[2..], NBitcoin.Network.Main);
            }
            else if (line.StartsWith("r="))
            {
                // 029e03a901b85534ff1e92c43c74431f7ce72046060fcf7a95c37e148f78c77255|66051x263430x1800|1|20|3$039e03a901b85534ff1e92c43c74431f7ce72046060fcf7a95c37e148f78c77255|197637x395016x2314|2|30|4
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("r line without invoice line");
                }

                var routingInfo = new RoutingInfoCollection();
                var routingInfoStrings = line[2..].Split('$');
                foreach (var routingInfoString in routingInfoStrings)
                {
                    var routingInfoParts = routingInfoString.Split('|');
                    if (routingInfoParts.Length != 5)
                    {
                        throw new InvalidOperationException("Invalid routing info");
                    }

                    var pubKey = new PubKey(GetBytes(routingInfoParts[0]));
                    var shortChannelId = ShortChannelId.Parse(routingInfoParts[1]);
                    var feeBaseMsat = int.Parse(routingInfoParts[2]);
                    var feeProportionalMillionths = int.Parse(routingInfoParts[3]);
                    var cltvExpiryDelta = short.Parse(routingInfoParts[4]);

                    routingInfo.Add(new RoutingInfo(pubKey, shortChannelId, feeBaseMsat, feeProportionalMillionths, cltvExpiryDelta));
                }

                currentInvoice.ExpectedRoutingInfo = routingInfo;
            }
            else if (line.StartsWith("9="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("f line without invoice line");
                }

                currentInvoice.ExpectedFeatures = Features.DeserializeFromBytes(GetBytes(line[2..]));
            }
            else if (line.StartsWith("m="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("m line without invoice line");
                }

                currentInvoice.ExpectedMetadata = GetBytes(line[2..]);
            }
            else if (line.Length == 0)
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("end line without invoice line");
                }

                testInvoice.Add(currentInvoice);
            }
        }

        return testInvoice;
    }
}