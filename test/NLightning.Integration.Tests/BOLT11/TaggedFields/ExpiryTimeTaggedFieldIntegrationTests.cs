namespace NLightning.Integration.Tests.BOLT11.TaggedFields;

using Bolt11.Models.TaggedFields;
using Domain.Utils;

public class ExpiryTimeTaggedFieldIntegrationTests
{
    private const int KnownExpirySeconds = 3600; // 1 hour

    private static (byte[] Buffer, int FieldOffsetBits, short Length) BuildBuffer(
        int prePadBits, int postPadBits, int expirySeconds)
    {
        var field = new ExpiryTimeTaggedField(expirySeconds);
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
        var (buffer, _, length) = BuildBuffer(prePadBits: 0, postPadBits: 7, KnownExpirySeconds);
        var reader = new BitReader(buffer);

        var parsed = ExpiryTimeTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownExpirySeconds, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Unaligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 13, postPadBits: 11, KnownExpirySeconds);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = ExpiryTimeTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownExpirySeconds, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Aligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 16, postPadBits: 11, KnownExpirySeconds);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = ExpiryTimeTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownExpirySeconds, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Unaligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 5, postPadBits: 3, KnownExpirySeconds);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = ExpiryTimeTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownExpirySeconds, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Aligned()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 8, postPadBits: 3, KnownExpirySeconds);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = ExpiryTimeTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownExpirySeconds, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_At_End_Of_Buffer()
    {
        var (buffer, fieldOffsetBits, length) = BuildBuffer(prePadBits: 7, postPadBits: 0, KnownExpirySeconds);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        var parsed = ExpiryTimeTaggedField.FromBitReader(reader, length);

        Assert.Equal(KnownExpirySeconds, parsed.Value);
    }
}