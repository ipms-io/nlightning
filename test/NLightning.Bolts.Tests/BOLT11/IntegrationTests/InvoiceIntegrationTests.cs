using System.Text;
using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT11.IntegrationTests;

using Bolts.BOLT11;
using Bolts.BOLT11.Enums;
using Bolts.BOLT11.Types;
using Common.Crypto.Hashes;
using Common.Exceptions;
using Common.Managers;
using Common.Node;
using Common.Types;
using TestCollections;
using static Utils.TestUtils;

[Collection(SecureKeyManagerCollection.NAME)]
public class InvoiceIntegrationTests : IDisposable
{
    private static readonly PubKey s_expectedPayeePubkey = new("03e7156ae33b0a208d0744199163177e909e80176e55d97a2f221ede0f934dd9ad");
    private readonly Network _originalConfigNetwork = Network.MAIN_NET;

    [Fact]
    public void Given_ValidInvoiceString_When_Decoding_Then_InvoiceIsCorrect()
    {
        // Arrange
        var testInvoices = ReadTestInvoices("BOLT11/Invoices/ValidInvoices.txt");

        // Act
        foreach (var testInvoice in testInvoices)
        {
            // Arrange
            ConfigManager.Instance.Network = testInvoice.ExpectedNetwork!.Value;

            var invoice = Invoice.Decode(testInvoice.INVOICE_STRING);

            // Assert
            Assert.Equal(testInvoice.ExpectedNetwork, invoice.Network);
            Assert.Equal(testInvoice.ExpectedAmountMilliSats, invoice.AmountMilliSats);
            Assert.Equal(testInvoice.ExpectedTimestamp, invoice.Timestamp);

            foreach (var taggedField in testInvoice.EXPECTED_TAGGED_FIELDS)
            {
                switch (taggedField.Key)
                {
                    case TaggedFieldTypes.PAYMENT_SECRET:
                        Assert.Equal(taggedField.Value, invoice.PaymentSecret);
                        break;
                    case TaggedFieldTypes.PAYMENT_HASH:
                        Assert.Equal(taggedField.Value, invoice.PaymentHash);
                        break;
                    case TaggedFieldTypes.DESCRIPTION_HASH:
                        Assert.Equal(taggedField.Value, invoice.DescriptionHash);
                        break;
                    case TaggedFieldTypes.FALLBACK_ADDRESS:
                        Assert.Equal(taggedField.Value, invoice.FallbackAddresses?.FirstOrDefault());
                        break;
                    case TaggedFieldTypes.DESCRIPTION:
                        Assert.Equal(taggedField.Value, invoice.Description);
                        break;
                    case TaggedFieldTypes.EXPIRY_TIME:
                        Assert.Equal(taggedField.Value, invoice.ExpiryDate);
                        break;
                    case TaggedFieldTypes.ROUTING_INFO:
                        Assert.NotNull(invoice.RoutingInfos);
                        var expectedRoutingInfo = taggedField.Value as RoutingInfoCollection ?? throw new NullReferenceException("TaggedFieldTypes.ROUTING_INFO is null");
                        Assert.Equal(expectedRoutingInfo.Count, invoice.RoutingInfos.Count);

                        for (var i = 0; i < expectedRoutingInfo.Count; i++)
                        {
                            Assert.Equal(expectedRoutingInfo[i].PubKey, invoice.RoutingInfos[i].PubKey);
                            Assert.Equal(expectedRoutingInfo[i].ShortChannelId, invoice.RoutingInfos[i].ShortChannelId);
                            Assert.Equal(expectedRoutingInfo[i].FeeBaseMsat, invoice.RoutingInfos[i].FeeBaseMsat);
                            Assert.Equal(expectedRoutingInfo[i].FeeProportionalMillionths, invoice.RoutingInfos[i].FeeProportionalMillionths);
                            Assert.Equal(expectedRoutingInfo[i].CltvExpiryDelta, invoice.RoutingInfos[i].CltvExpiryDelta);
                        }
                        break;
                    case TaggedFieldTypes.FEATURES:
                        var expectedFeatures = taggedField.Value as FeatureSet;
                        Assert.NotNull(expectedFeatures);
                        Assert.NotNull(invoice.Features);
                        Assert.True(expectedFeatures.IsCompatible(invoice.Features));
                        break;
                    case TaggedFieldTypes.METADATA:
                        Assert.Equal(taggedField.Value, invoice.Metadata);
                        break;
                    case TaggedFieldTypes.MIN_FINAL_CLTV_EXPIRY:
                        Assert.Equal(taggedField.Value, invoice.MinFinalCltvExpiry);
                        break;
                    case TaggedFieldTypes.PAYEE_PUB_KEY:
                        Assert.Equal(taggedField.Value, invoice.PayeePubKey);
                        break;
                    default:
                        continue;
                }
            }

            Assert.Equal(s_expectedPayeePubkey, invoice.PayeePubKey);
        }
    }

    [Theory]
    [InlineData("Error in Bech32 string", "lnbc2500u1pvjluezpp5qqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqypqdpquwpc4curk03c9wlrswe78q4eyqc7d8d0xqzpuyk0sg5g70me25alkluzd2x62aysf2pyy8edtjeevuv4p2d5p76r4zkmneet7uvyakky2zr4cusd45tftc9c5fh0nnqpnl2jfll544esqchsrnt")]
    [InlineData("Missing prefix in invoice", "pvjluezpp5qqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqypqdpquwpc4curk03c9wlrswe78q4eyqc7d8d0xqzpuyk0sg5g70me25alkluzd2x62aysf2pyy8edtjeevuv4p2d5p76r4zkmneet7uvyakky2zr4cusd45tftc9c5fh0nnqpnl2jfll544esqchsrny")]
    [InlineData("Impossible to recover the public key", "lnbc2500u1pvjluezpp5qqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqypqdq5xysxxatsyp3k7enxv4jsxqzpusp5zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zygs9qrsgqwgt7mcn5yqw3yx0w94pswkpq6j9uh6xfqqqtsk4tnarugeektd4hg5975x9am52rz4qskukxdmjemg92vvqz8nvmsye63r5ykel43pgz7zq0g2")]
    [InlineData("Specified argument was out of the range of valid values", "lnbc1pvjluezpp5qqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqypqdpl2pkx2ctnv5sxxmmwwd5kgetjypeh2ursdae8g6na6hlh")]
    [InlineData("Invalid amount format in invoice", "lnbc2500x1pvjluezpp5qqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqypqdq5xysxxatsyp3k7enxv4jsxqzpusp5zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zygs9qrsgqrrzc4cvfue4zp3hggxp47ag7xnrlr8vgcmkjxk3j5jqethnumgkpqp23z9jclu3v0a7e0aruz366e9wqdykw6dxhdzcjjhldxq0w6wgqcnu43j")]
    [InlineData("Invalid pico amount in invoice", "lnbc2500000001p1pvjluezpp5qqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqypqdq5xysxxatsyp3k7enxv4jsxqzpusp5zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zygs9qrsgq0lzc236j96a95uv0m3umg28gclm5lqxtqqwk32uuk4k6673k6n5kfvx3d2h8s295fad45fdhmusm8sjudfhlf6dcsxmfvkeywmjdkxcp99202x")]
    public void Given_InvalidInvoiceString_When_Decoding_Then_ExceptionIsThrown(string errorMessage, string? invoiceString)
    {
        // Arrange
        ConfigManager.Instance.Network = NBitcoin.Network.Main;

        // Act & Assert
        var exception = Assert.Throws<InvoiceSerializationException>(() => Invoice.Decode(invoiceString));
        Assert.Equal("Error decoding invoice", exception.Message);
        Assert.Contains(errorMessage, exception.InnerException?.Message);
    }

    [Fact]
    public void Given_ValidDecodedInvoice_When_Encoding_Then_InvoiceStringIsCorrect()
    {
        // Arrange
        var testInvoices = ReadTestInvoices("BOLT11/Invoices/ValidInvoices.txt");

        foreach (var testInvoice in testInvoices)
        {
            if (testInvoice.IgnoreEncode) continue;

            var invoice = new Invoice(testInvoice.ExpectedNetwork!.Value, testInvoice.ExpectedAmountMilliSats, testInvoice.ExpectedTimestamp);

            foreach (var taggedField in testInvoice.EXPECTED_TAGGED_FIELDS)
            {
                switch (taggedField.Key)
                {
                    case TaggedFieldTypes.PAYMENT_SECRET:
                        invoice.PaymentSecret = taggedField.Value as uint256 ?? throw new InvalidCastException("Unable to cast taggedField to uint256");
                        break;
                    case TaggedFieldTypes.PAYMENT_HASH:
                        invoice.PaymentHash = taggedField.Value as uint256 ?? throw new InvalidCastException("Unable to cast taggedField to uint256");
                        break;
                    case TaggedFieldTypes.DESCRIPTION_HASH:
                        invoice.DescriptionHash = taggedField.Value as uint256;
                        break;
                    case TaggedFieldTypes.FALLBACK_ADDRESS:
                        invoice.FallbackAddresses = [taggedField.Value as BitcoinAddress ?? throw new InvalidCastException("Unable to cast taggedField to BitcoinAddress")];
                        break;
                    case TaggedFieldTypes.DESCRIPTION:
                        invoice.Description = taggedField.Value as string;
                        break;
                    case TaggedFieldTypes.EXPIRY_TIME:
                        invoice.ExpiryDate = taggedField.Value as DateTimeOffset? ?? throw new InvalidCastException("Unable to cast taggedField to DateTimeOffset");
                        break;
                    case TaggedFieldTypes.ROUTING_INFO:
                        invoice.RoutingInfos = taggedField.Value as RoutingInfoCollection;
                        break;
                    case TaggedFieldTypes.FEATURES:
                        invoice.Features = taggedField.Value as FeatureSet;
                        break;
                    case TaggedFieldTypes.METADATA:
                        invoice.Metadata = taggedField.Value as byte[];
                        break;
                    case TaggedFieldTypes.MIN_FINAL_CLTV_EXPIRY:
                        invoice.MinFinalCltvExpiry = taggedField.Value as ushort?;
                        break;
                    case TaggedFieldTypes.PAYEE_PUB_KEY:
                        invoice.PayeePubKey = taggedField.Value as PubKey;
                        break;
                    default:
                        continue;
                }
            }

            // Act
            var invoiceString = invoice.Encode();

            // Assert
            Assert.Equal(testInvoice.INVOICE_STRING?.ToLowerInvariant(), invoiceString);
        }
    }

    private class TestInvoice(string? invoiceString)
    {
        public readonly string? INVOICE_STRING = invoiceString;
        public Network? ExpectedNetwork;
        public ulong? ExpectedAmountMilliSats;
        public long? ExpectedTimestamp;
        public readonly Dictionary<TaggedFieldTypes, object> EXPECTED_TAGGED_FIELDS = [];
        public bool IgnoreEncode;
    }

    private static List<TestInvoice> ReadTestInvoices(string filePath)
    {
        var testInvoice = new List<TestInvoice>();
        TestInvoice? currentInvoice = null;
        using var sha256 = new Sha256();

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

                var data = GetBytes(line[2..]);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(data);
                }

                currentInvoice.EXPECTED_TAGGED_FIELDS.Add(TaggedFieldTypes.PAYMENT_HASH, new uint256(data));
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

                currentInvoice.EXPECTED_TAGGED_FIELDS.Add(TaggedFieldTypes.PAYMENT_SECRET, new uint256(data));
            }
            else if (line.StartsWith("d="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("d line without invoice line");
                }

                currentInvoice.EXPECTED_TAGGED_FIELDS.Add(TaggedFieldTypes.DESCRIPTION, line[2..]);
            }
            else if (line.StartsWith("x="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("x line without invoice line");
                }

                currentInvoice.EXPECTED_TAGGED_FIELDS.Add(TaggedFieldTypes.EXPIRY_TIME, DateTimeOffset.FromUnixTimeSeconds(currentInvoice.ExpectedTimestamp!.Value + long.Parse(line[2..])));
            }
            else if (line.StartsWith("h="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("h line without invoice line");
                }

                var hash = new byte[32];
                sha256.AppendData(Encoding.UTF8.GetBytes(line[2..]));
                sha256.GetHashAndReset(hash);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(hash);
                }

                currentInvoice.EXPECTED_TAGGED_FIELDS.Add(TaggedFieldTypes.DESCRIPTION_HASH, new uint256(hash));
            }
            else if (line.StartsWith("f="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("f line without invoice line");
                }

                var network = NBitcoin.Network.Main;
                if (currentInvoice.ExpectedNetwork == null || currentInvoice.ExpectedNetwork == Network.SIG_NET)
                {
                    throw new Exception("Invalid network");
                }

                if (currentInvoice.ExpectedNetwork == Network.TEST_NET)
                {
                    network = NBitcoin.Network.TestNet;
                }
                else if (currentInvoice.ExpectedNetwork == Network.REG_TEST)
                {
                    network = NBitcoin.Network.RegTest;
                }

                currentInvoice.EXPECTED_TAGGED_FIELDS.Add(TaggedFieldTypes.FALLBACK_ADDRESS, BitcoinAddress.Create(line[2..], network));
            }
            else if (line.StartsWith("r="))
            {
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

                currentInvoice.EXPECTED_TAGGED_FIELDS.Add(TaggedFieldTypes.ROUTING_INFO, routingInfo);
            }
            else if (line.StartsWith("9="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("f line without invoice line");
                }

                currentInvoice.EXPECTED_TAGGED_FIELDS.Add(TaggedFieldTypes.FEATURES, FeatureSet.DeserializeFromBytes(GetBytes(line[2..])));
            }
            else if (line.StartsWith("m="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("m line without invoice line");
                }

                currentInvoice.EXPECTED_TAGGED_FIELDS.Add(TaggedFieldTypes.METADATA, GetBytes(line[2..]));
            }
            else if (line.StartsWith("c="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("c line without invoice line");
                }

                currentInvoice.EXPECTED_TAGGED_FIELDS.Add(TaggedFieldTypes.MIN_FINAL_CLTV_EXPIRY, ushort.Parse(line[2..]));
            }
            else if (line.StartsWith("ignoreEncode="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("c line without invoice line");
                }

                currentInvoice.IgnoreEncode = bool.Parse(line[13..]);
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

    public void Dispose()
    {
        ConfigManager.Instance.Network = _originalConfigNetwork;
    }
}