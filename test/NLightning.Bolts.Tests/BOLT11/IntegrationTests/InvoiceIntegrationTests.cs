using System.Text;
using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT11.IntegrationTests;

using Bolts.BOLT11.Types;
using Bolts.BOLT9;
using Common.Types;
using NLightning.Bolts.BOLT11;
using NLightning.Bolts.BOLT8.Constants;
using NLightning.Bolts.BOLT8.Hashes;
// using Common.Utils;
using static Utils.TestUtils;

public class InvoiceIntegrationTests
{
    [Fact]
    public void Given_ValidInvoiceString_When_Decoding_Then_InvoiceIsCorrect()
    {
        // Arrange
        var testInvoices = ReadTestInvoices("BOLT11/Invoices/ValidInvoices.txt");

        // Act
        foreach (var testInvoice in testInvoices)
        {
            var invoice = Invoice.Decode(testInvoice.INVOICE_STRING);

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

    [Fact]
    public void Given_ValidDecodedInvoice_When_Encoding_Then_InvoiceStringIsCorrect()
    {
        // Arrange
        const string HEX_PRIVATE_KEY = "e126f68f7eafcc8b74f54d269fe206be715000f94dac067d1c04a8ca3b2db734";
        var privateKeyBytes = NBitcoin.DataEncoders.Encoders.Hex.DecodeData(HEX_PRIVATE_KEY);
        var key = new Key(privateKeyBytes);

        var testInvoices = ReadTestInvoices("BOLT11/Invoices/ValidInvoices.txt");

        // Act
        foreach (var testInvoice in testInvoices)
        {
            var invoice = new Invoice(testInvoice.ExpectedNetwork!.Value, key, testInvoice.ExpectedAmountMilliSats, testInvoice.ExpectedTimestamp);

            if (testInvoice.ExpectedPaymentSecret != null)
            {
                invoice.PaymentSecret = testInvoice.ExpectedPaymentSecret;
            }

            if (testInvoice.ExpectedPaymentHash != null)
            {
                invoice.PaymentHash = testInvoice.ExpectedPaymentHash;
            }

            if (testInvoice.ExpectedDescriptionHash != null)
            {
                invoice.DescriptionHash = testInvoice.ExpectedDescriptionHash;
            }

            if (testInvoice.ExpectedFallbackAddress != null)
            {
                invoice.FallbackAddresses = [testInvoice.ExpectedFallbackAddress];
            }

            if (testInvoice.ExpectedDescription != null)
            {
                invoice.Description = testInvoice.ExpectedDescription;
            }

            if (testInvoice.ExpectedMetadata != null)
            {
                invoice.Metadata = testInvoice.ExpectedMetadata;
            }

            if (testInvoice.ExpectedExpiryTime != null)
            {
                invoice.ExpiryDate = testInvoice.ExpectedExpiryTime;
            }

            if (testInvoice.ExpectedMinFinalCltvExpiry != null)
            {
                invoice.MinFinalCltvExpiry = testInvoice.ExpectedMinFinalCltvExpiry;
            }

            if (testInvoice.ExpectedRoutingInfo != null)
            {
                invoice.RoutingInfos = testInvoice.ExpectedRoutingInfo;
            }

            if (testInvoice.ExpectedFeatures != null)
            {
                invoice.Features = testInvoice.ExpectedFeatures;
            }

            var invoiceString = invoice.Encode();

            //357wnc5r2ueh7ck6q93dj32dlqnls087fxdwk8qakdyafkq3yap9us6v52vjjsrvywa6rt52cm9r9zqt8r2t7mlcwspyetp5h2tztugp9lfyql
            //zv9pyhclz5mcmzlmlpvlk3huk5lnfxsjzamr8xy744tha7fvvrzpyg0x44z4wpf73y7qygaamrwvwlaxy5prpd4nqytj53u267j64acqqqs7qvgn
            // Assert
            Assert.Equal(testInvoice.INVOICE_STRING, invoiceString);
        }
    }

    private class TestInvoice(string invoiceString)
    {
        public readonly string INVOICE_STRING = invoiceString;
        public Common.Network? ExpectedNetwork;
        public ulong? ExpectedAmountMilliSats;
        public long? ExpectedTimestamp;
        public uint256? ExpectedPaymentHash;
        public uint256? ExpectedPaymentSecret;
        public Features? ExpectedFeatures;
        public RoutingInfoCollection? ExpectedRoutingInfo;
        public DateTimeOffset? ExpectedExpiryTime;
        public BitcoinAddress? ExpectedFallbackAddress;
        public string? ExpectedDescription;
        public uint256? ExpectedDescriptionHash;
        public ushort? ExpectedMinFinalCltvExpiry;
        public byte[]? ExpectedMetadata;
    }

    private static List<TestInvoice> ReadTestInvoices(string filePath)
    {
        var testInvoice = new List<TestInvoice>();
        TestInvoice? currentInvoice = null;
        var hasher = new Sha256();

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

                currentInvoice.ExpectedNetwork = new Common.Network(line[8..]);
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

                var data = GetBytes(line[2..]);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(data);
                }

                currentInvoice.ExpectedPaymentHash = new uint256(data);
            }
            else if (line.StartsWith("s="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("s line without invoice line");
                }

                var data = GetBytes(line[2..]);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(data);
                }

                currentInvoice.ExpectedPaymentSecret = new uint256(data);
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

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(hash);
                }

                currentInvoice.ExpectedDescriptionHash = new uint256(hash);
            }
            else if (line.StartsWith("f="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("f line without invoice line");
                }

                currentInvoice.ExpectedFallbackAddress = BitcoinAddress.Create(line[2..], Network.Main);
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
            else if (line.StartsWith("c="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("c line without invoice line");
                }

                currentInvoice.ExpectedMinFinalCltvExpiry = ushort.Parse(line[2..]);
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