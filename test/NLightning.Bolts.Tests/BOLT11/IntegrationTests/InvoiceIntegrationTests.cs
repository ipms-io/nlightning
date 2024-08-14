using System.Text;
using NBitcoin;
using NLightning.Common.Managers;

namespace NLightning.Bolts.Tests.BOLT11.IntegrationTests;

using Bolts.BOLT11;
using Bolts.BOLT11.Enums;
using Bolts.BOLT11.Types;
using Bolts.BOLT8.Constants;
using Bolts.BOLT8.Hashes;
using Bolts.BOLT9;
using Common.Types;
using static Utils.TestUtils;

public class InvoiceIntegrationTests
{
    private static readonly PubKey s_expectedPayeePubkey = new PubKey("03e7156ae33b0a208d0744199163177e909e80176e55d97a2f221ede0f934dd9ad");

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

            foreach (var taggedField in testInvoice.ExpectedTaggedFields)
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
                    // case TaggedFieldTypes.FALLBACK_ADDRESS:
                    //     Assert.Equal(taggedField.Value, invoice.FallbackAddresses?.FirstOrDefault());
                    //     break;
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
                        var expectedFeatures = taggedField.Value as Features;
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

    [Fact]
    public void Given_ValidDecodedInvoice_When_Encoding_Then_InvoiceStringIsCorrect()
    {
        // Arrange
        const string HEX_PRIVATE_KEY = "e126f68f7eafcc8b74f54d269fe206be715000f94dac067d1c04a8ca3b2db734";
        var privateKeyBytes = NBitcoin.DataEncoders.Encoders.Hex.DecodeData(HEX_PRIVATE_KEY);
        SecureKeyManager.Initialize(privateKeyBytes);

        var testInvoices = ReadTestInvoices("BOLT11/Invoices/ValidInvoices.txt");

        // Act
        foreach (var testInvoice in testInvoices)
        {
            // TODO: Remove once address is fixed
            if (testInvoice.ExpectedTaggedFields.ContainsKey(TaggedFieldTypes.FALLBACK_ADDRESS)) continue;

            var invoice = new Invoice(testInvoice.ExpectedNetwork!.Value, testInvoice.ExpectedAmountMilliSats, testInvoice.ExpectedTimestamp);

            foreach (var taggedField in testInvoice.ExpectedTaggedFields)
            {
                switch (taggedField.Key)
                {
                    case TaggedFieldTypes.PAYMENT_SECRET:
                        invoice.PaymentSecret = taggedField.Value as uint256;
                        break;
                    case TaggedFieldTypes.PAYMENT_HASH:
                        invoice.PaymentHash = taggedField.Value as uint256 ?? throw new NullReferenceException("TaggedFieldTypes.PAYMENT_HASH is null");
                        break;
                    case TaggedFieldTypes.DESCRIPTION_HASH:
                        invoice.DescriptionHash = taggedField.Value as uint256;
                        break;
                    case TaggedFieldTypes.FALLBACK_ADDRESS:
                        invoice.FallbackAddresses = [taggedField.Value as BitcoinAddress ?? throw new NullReferenceException("TaggedFieldTypes.FALLBACK_ADDRESS is null")];
                        break;
                    case TaggedFieldTypes.DESCRIPTION:
                        invoice.Description = taggedField.Value as string;
                        break;
                    case TaggedFieldTypes.EXPIRY_TIME:
                        invoice.ExpiryDate = taggedField.Value as DateTimeOffset?;
                        break;
                    case TaggedFieldTypes.ROUTING_INFO:
                        invoice.RoutingInfos = taggedField.Value as RoutingInfoCollection;
                        break;
                    case TaggedFieldTypes.FEATURES:
                        invoice.Features = taggedField.Value as Features;
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

            var invoiceString = invoice.Encode();

            // Assert
            Assert.Equal(testInvoice.INVOICE_STRING, invoiceString);
        }
    }

    private class TestInvoice(string invoiceString)
    {
        public readonly string INVOICE_STRING = invoiceString;
        public Network? ExpectedNetwork;
        public ulong? ExpectedAmountMilliSats;
        public long? ExpectedTimestamp;
        public Dictionary<TaggedFieldTypes, object> ExpectedTaggedFields = [];
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

                currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.PAYMENT_HASH, new uint256(data));
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

                currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.PAYMENT_SECRET, new uint256(data));
            }
            else if (line.StartsWith("d="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("d line without invoice line");
                }

                currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.DESCRIPTION, line[2..]);
            }
            else if (line.StartsWith("x="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("x line without invoice line");
                }

                currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.EXPIRY_TIME, DateTimeOffset.FromUnixTimeSeconds(currentInvoice.ExpectedTimestamp!.Value + long.Parse(line[2..])));
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

                currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.DESCRIPTION_HASH, new uint256(hash));
            }
            else if (line.StartsWith("f="))
            {
                // TODO: Get network from context first
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("f line without invoice line");
                }
                //
                // var network = Network.Main;
                // if (currentInvoice.ExpectedNetwork == null || currentInvoice.ExpectedNetwork == Common.Network.SIG_NET)
                // {
                //     throw new Exception("Invalid network");
                // } 
                //
                // if (currentInvoice.ExpectedNetwork == Common.Network.TEST_NET)
                // {
                //     network = Network.TestNet;
                // } 
                // else if (currentInvoice.ExpectedNetwork == Common.Network.REG_TEST)
                // {
                //     network = Network.RegTest;
                // }
                //
                // currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.FALLBACK_ADDRESS, BitcoinAddress.Create(line[2..], network));
                currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.FALLBACK_ADDRESS, null!);
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

                currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.ROUTING_INFO, routingInfo);
            }
            else if (line.StartsWith("9="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("f line without invoice line");
                }

                currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.FEATURES, Features.DeserializeFromBytes(GetBytes(line[2..])));
            }
            else if (line.StartsWith("m="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("m line without invoice line");
                }

                currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.METADATA, GetBytes(line[2..]));
            }
            else if (line.StartsWith("c="))
            {
                if (currentInvoice == null)
                {
                    throw new InvalidOperationException("c line without invoice line");
                }

                currentInvoice.ExpectedTaggedFields.Add(TaggedFieldTypes.MIN_FINAL_CLTV_EXPIRY, ushort.Parse(line[2..]));
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