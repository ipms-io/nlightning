namespace NLightning.Integration.Tests.BOLT11.TaggedFields;

using Bolt11.Models.TaggedFields;
using Domain.Channels.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Models;
using Domain.Utils;

public class RoutingInfoTaggedFieldIntegrationTests
{
    private static RoutingInfo BuildKnownRoutingInfo()
    {
        // 33-byte compact pubkey starting with a valid prefix (0x02)
        var pubkey = new CompactPubKey([
            0x02,
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
            0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
            0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
        ]);

        // ShortChannelId created from human-readable form
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
            // Vary the SCID slightly to ensure entries are distinct
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

    private static (byte[] Buffer, int FieldOffsetBits, short Length, RoutingInfoCollection Expected) BuildBuffer(
        int prePadBits, int postPadBits, RoutingInfoCollection routingInfos)
    {
        var field = new RoutingInfoTaggedField(routingInfos);
        var fieldBits = field.Length * 5;
        var totalBits = prePadBits + fieldBits + postPadBits;

        using var writer = new BitWriter(totalBits);

        if (prePadBits > 0)
            writer.SkipBits(prePadBits);

        field.WriteToBitWriter(writer);

        if (postPadBits > 0)
            writer.SkipBits(postPadBits);

        return (writer.ToArray(), prePadBits, field.Length, routingInfos);
    }

    private static void AssertRoutingInfosEqual(RoutingInfoCollection expected, RoutingInfoCollection actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].CompactPubKey, actual[i].CompactPubKey);
            Assert.Equal(expected[i].ShortChannelId, actual[i].ShortChannelId);
            Assert.Equal(expected[i].FeeBaseMsat, actual[i].FeeBaseMsat);
            Assert.Equal(expected[i].FeeProportionalMillionths, actual[i].FeeProportionalMillionths);
            Assert.Equal(expected[i].CltvExpiryDelta, actual[i].CltvExpiryDelta);
        }
    }

    [Fact]
    public void FromBitReader_Reads_From_Beginning_Of_Buffer()
    {
        var collection = BuildKnownCollection(1);
        var (buffer, _, length, expected) = BuildBuffer(prePadBits: 0, postPadBits: 7, collection);
        var reader = new BitReader(buffer);

        var parsed = RoutingInfoTaggedField.FromBitReader(reader, length);

        Assert.NotNull(parsed);
        AssertRoutingInfosEqual(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Unaligned()
    {
        var collection = BuildKnownCollection(1);
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 13, postPadBits: 11, collection);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = RoutingInfoTaggedField.FromBitReader(reader, length);

        Assert.NotNull(parsed);
        AssertRoutingInfosEqual(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Aligned()
    {
        var collection = BuildKnownCollection(1);
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 16, postPadBits: 11, collection);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = RoutingInfoTaggedField.FromBitReader(reader, length);

        Assert.NotNull(parsed);
        AssertRoutingInfosEqual(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Unaligned()
    {
        var collection = BuildKnownCollection(1);
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 5, postPadBits: 3, collection);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = RoutingInfoTaggedField.FromBitReader(reader, length);

        Assert.NotNull(parsed);
        AssertRoutingInfosEqual(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Aligned()
    {
        var collection = BuildKnownCollection(1);
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 8, postPadBits: 3, collection);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = RoutingInfoTaggedField.FromBitReader(reader, length);

        Assert.NotNull(parsed);
        AssertRoutingInfosEqual(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_At_End_Of_Buffer()
    {
        var collection = BuildKnownCollection(1);
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 7, postPadBits: 0, collection);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = RoutingInfoTaggedField.FromBitReader(reader, length);

        Assert.NotNull(parsed);
        AssertRoutingInfosEqual(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Multiple_Entries_Correctly()
    {
        var collection = BuildKnownCollection(2);
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 13, postPadBits: 7, collection);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = RoutingInfoTaggedField.FromBitReader(reader, length);

        Assert.NotNull(parsed);
        AssertRoutingInfosEqual(expected, parsed.Value);
    }
}