namespace NLightning.Integration.Tests.BOLT11.TaggedFields;

using Bolt11.Models.TaggedFields;
using Domain.Utils;

public class DescriptionTaggedFieldIntegrationTests
{
    private const string KnownDescription = "The quick brown fox jumps over 13 lazy dogs.";

    private static (byte[] Buffer, int FieldOffsetBits, short Length) BuildBuffer(
        int prePadBits, int postPadBits, string description)
    {
        var field = new DescriptionTaggedField(description);
        var fieldBits = field.Length * 5;
        var totalBits = prePadBits + fieldBits + postPadBits;

        using var writer = new BitWriter(totalBits);

        if (prePadBits > 0)
            writer.SkipBits(prePadBits);

        field.WriteToBitWriter(writer);

        if (postPadBits > 0)
            writer.SkipBits(postPadBits);

        return (writer.ToArray(), prePadBits, field.Length);
    }

    [Fact]
    public void FromBitReader_Reads_From_Beginning_Of_Buffer()
    {
        var (buffer, _, length) = BuildBuffer(prePadBits: 0, postPadBits: 7, KnownDescription);
        var reader = new BitReader(buffer);

        var parsed = DescriptionTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownDescription, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Unaligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 13, postPadBits: 11, KnownDescription);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = DescriptionTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownDescription, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Aligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 16, postPadBits: 11, KnownDescription);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = DescriptionTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownDescription, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Unaligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 5, postPadBits: 3, KnownDescription);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = DescriptionTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownDescription, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Aligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 8, postPadBits: 3, KnownDescription);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = DescriptionTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownDescription, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_At_End_Of_Buffer()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 7, postPadBits: 0, KnownDescription);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = DescriptionTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownDescription, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Empty_Description()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 10, postPadBits: 6, string.Empty);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = DescriptionTaggedField.FromBitReader(reader, length);

        Assert.Equal(string.Empty, parsed.Value);
    }
}