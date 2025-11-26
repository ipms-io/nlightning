namespace NLightning.Integration.Tests.BOLT11.TaggedFields;

using Bolt11.Models.TaggedFields;
using Domain.Utils;

public class MetadataTaggedFieldIntegrationTests
{
    private static readonly byte[] s_knownMetadata =
    [
        // 31 bytes with a deterministic pattern (not a multiple of 5 bits to exercise padding)
        0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
        0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF,
        0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80,
        0x90, 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0
    ];

    private static (byte[] Buffer, int FieldOffsetBits, short Length, byte[] Payload) BuildBuffer(
        int prePadBits, int postPadBits, byte[] payload)
    {
        var field = new MetadataTaggedField(payload);
        var fieldBits = field.Length * 5;
        var totalBits = prePadBits + fieldBits + postPadBits;

        using var writer = new BitWriter(totalBits);

        if (prePadBits > 0)
            writer.SkipBits(prePadBits);

        field.WriteToBitWriter(writer);

        if (postPadBits > 0)
            writer.SkipBits(postPadBits);

        return (writer.ToArray(), prePadBits, field.Length, payload);
    }

    [Fact]
    public void FromBitReader_Reads_From_Beginning_Of_Buffer()
    {
        var (buffer, _, length, expected) = BuildBuffer(prePadBits: 0, postPadBits: 7, s_knownMetadata);
        var reader = new BitReader(buffer);

        var parsed = MetadataTaggedField.FromBitReader(reader, length);

        Assert.Equal(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Unaligned()
    {
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 13, postPadBits: 11, s_knownMetadata);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = MetadataTaggedField.FromBitReader(reader, length);

        Assert.Equal(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Aligned()
    {
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 16, postPadBits: 11, s_knownMetadata);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = MetadataTaggedField.FromBitReader(reader, length);

        Assert.Equal(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Unaligned()
    {
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 5, postPadBits: 3, s_knownMetadata);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = MetadataTaggedField.FromBitReader(reader, length);

        Assert.Equal(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Aligned()
    {
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 8, postPadBits: 3, s_knownMetadata);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = MetadataTaggedField.FromBitReader(reader, length);

        Assert.Equal(expected, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_At_End_Of_Buffer()
    {
        var (buffer, fieldOffsetBits, length, expected) = BuildBuffer(prePadBits: 7, postPadBits: 0, s_knownMetadata);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = MetadataTaggedField.FromBitReader(reader, length);

        Assert.Equal(expected, parsed.Value);
    }
}
