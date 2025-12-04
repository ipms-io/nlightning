namespace NLightning.Bolt11.Tests.Models.TaggedFields;

using Bolt11.Models.TaggedFields;
using Constants;
using Domain.Channels.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Models;
using Domain.Utils;
using Enums;

public class RoutingInfoTaggedFieldTests
{
    private static RoutingInfo BuildKnownRoutingInfo()
    {
        var pubkey = new CompactPubKey([
            0x02,
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
            0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
            0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
        ]);

        var scid = ShortChannelId.Parse("539268x845x1");

        const int feeBaseMsat = 1000;
        const int feeProportionalMillionths = 250;
        const short cltvDelta = 40;

        return new RoutingInfo(pubkey, scid, feeBaseMsat, feeProportionalMillionths, cltvDelta);
    }

    private static RoutingInfoCollection BuildKnownCollection(int entries)
    {
        var col = new RoutingInfoCollection();
        for (var i = 0; i < entries; i++)
        {
            var baseRi = BuildKnownRoutingInfo();
            var variedScid = new ShortChannelId(baseRi.ShortChannelId.BlockHeight,
                                                baseRi.ShortChannelId.TransactionIndex + (uint)i,
                                                (ushort)(baseRi.ShortChannelId.OutputIndex + i));
            col.Add(new RoutingInfo(baseRi.CompactPubKey,
                                    variedScid,
                                    baseRi.FeeBaseMsat + i,
                                    baseRi.FeeProportionalMillionths + i,
                                    (short)(baseRi.CltvExpiryDelta + i)));
        }

        return col;
    }

    [Fact]
    public void Constructor_FromValue_SetsPropertiesCorrectly()
    {
        // Arrange
        var collection = BuildKnownCollection(1);

        // Act
        var field = new RoutingInfoTaggedField(collection);

        // Assert
        Assert.Equal(TaggedFieldTypes.RoutingInfo, field.Type);
        // Length should be (n * 408 + n * 2) / 5 for n entries
        Assert.Equal((short)((1 * TaggedFieldConstants.RoutingInfoLength + 1 * 2) / 5), field.Length);
        Assert.True(field.IsValid());
    }

    [Fact]
    public void WriteToBitWriter_And_FromBitReader_RoundTrip_SingleEntry()
    {
        // Arrange
        var expected = BuildKnownCollection(1);
        var field = new RoutingInfoTaggedField(expected);
        var writer = new BitWriter(field.Length * 5);

        // Act
        field.WriteToBitWriter(writer);
        var reader = new BitReader(writer.ToArray());
        var parsed = RoutingInfoTaggedField.FromBitReader(reader, field.Length);

        // Assert
        Assert.NotNull(parsed);
        Assert.Equal(expected.Count, parsed.Value.Count);
        Assert.Equal(expected[0].CompactPubKey, parsed.Value[0].CompactPubKey);
        Assert.Equal(expected[0].ShortChannelId, parsed.Value[0].ShortChannelId);
        Assert.Equal(expected[0].FeeBaseMsat, parsed.Value[0].FeeBaseMsat);
        Assert.Equal(expected[0].FeeProportionalMillionths, parsed.Value[0].FeeProportionalMillionths);
        Assert.Equal(expected[0].CltvExpiryDelta, parsed.Value[0].CltvExpiryDelta);
    }

    [Fact]
    public void WriteToBitWriter_And_FromBitReader_RoundTrip_MultipleEntries()
    {
        // Arrange
        var expected = BuildKnownCollection(2);
        var field = new RoutingInfoTaggedField(expected);
        var writer = new BitWriter(field.Length * 5);

        // Act
        field.WriteToBitWriter(writer);
        var reader = new BitReader(writer.ToArray());
        var parsed = RoutingInfoTaggedField.FromBitReader(reader, field.Length);

        // Assert
        Assert.NotNull(parsed);
        Assert.Equal(expected.Count, parsed.Value.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].CompactPubKey, parsed.Value[i].CompactPubKey);
            Assert.Equal(expected[i].ShortChannelId, parsed.Value[i].ShortChannelId);
            Assert.Equal(expected[i].FeeBaseMsat, parsed.Value[i].FeeBaseMsat);
            Assert.Equal(expected[i].FeeProportionalMillionths, parsed.Value[i].FeeProportionalMillionths);
            Assert.Equal(expected[i].CltvExpiryDelta, parsed.Value[i].CltvExpiryDelta);
        }
    }

    [Fact]
    public void FromBitReader_ReturnsNull_When_LengthTooSmall()
    {
        // Arrange: smaller than one full entry (less than 408 bits)
        var buffer = new byte[(400 + 7) / 8];
        var reader = new BitReader(buffer);

        // Act
        var parsed = RoutingInfoTaggedField.FromBitReader(reader, 400 / 5);

        // Assert
        Assert.Null(parsed);
    }
}