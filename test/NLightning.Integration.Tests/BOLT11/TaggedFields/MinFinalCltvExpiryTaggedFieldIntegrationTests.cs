namespace NLightning.Integration.Tests.BOLT11.TaggedFields;

using Bolt11.Models.TaggedFields;
using Domain.Utils;

public class MinFinalCltvExpiryTaggedFieldIntegrationTests
{
    private const ushort KnownCltvExpiry = 144; // common default for CLTV delta

    private static (byte[] Buffer, int FieldOffsetBits, short Length) BuildBuffer(
        int prePadBits, int postPadBits, ushort cltvExpiry)
    {
        var field = new MinFinalCltvExpiryTaggedField(cltvExpiry);
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
        var (buffer, _, length) = BuildBuffer(prePadBits: 0, postPadBits: 7, KnownCltvExpiry);
        var reader = new BitReader(buffer);

        var parsed = MinFinalCltvExpiryTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownCltvExpiry, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Unaligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 13, postPadBits: 11, KnownCltvExpiry);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = MinFinalCltvExpiryTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownCltvExpiry, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Aligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 16, postPadBits: 11, KnownCltvExpiry);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = MinFinalCltvExpiryTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownCltvExpiry, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Unaligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 5, postPadBits: 3, KnownCltvExpiry);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = MinFinalCltvExpiryTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownCltvExpiry, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Aligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 8, postPadBits: 3, KnownCltvExpiry);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = MinFinalCltvExpiryTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownCltvExpiry, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_At_End_Of_Buffer()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 7, postPadBits: 0, KnownCltvExpiry);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = MinFinalCltvExpiryTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownCltvExpiry, parsed.Value);
    }
}