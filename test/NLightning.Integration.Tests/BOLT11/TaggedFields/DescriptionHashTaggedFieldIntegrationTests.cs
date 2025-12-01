using NBitcoin;

namespace NLightning.Integration.Tests.BOLT11.TaggedFields;

using Bolt11.Constants;
using Bolt11.Models.TaggedFields;
using Domain.Utils;

public class DescriptionHashTaggedFieldIntegrationTests
{
    private static readonly uint256 s_knownHash = new(
        new byte[]
        {
            // 32 bytes with a deterministic pattern
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F
        }
    );

    private static (byte[] Buffer, int FieldOffsetBits) BuildBuffer(int prePadBits, int postPadBits)
    {
        const int fieldBits = TaggedFieldConstants.HashLength * 5; // 52 groups of 5 bits = 260 bits
        var totalBits = prePadBits + fieldBits + postPadBits;

        using var writer = new BitWriter(totalBits);

        // Skip leading bits to start field at desired offset
        if (prePadBits > 0)
            writer.SkipBits(prePadBits);

        // Write the field using production code to ensure correct encoding
        var field = new DescriptionHashTaggedField(s_knownHash);
        field.WriteToBitWriter(writer);

        // Optionally add trailing padding bits
        if (postPadBits > 0)
            writer.SkipBits(postPadBits);

        return (writer.ToArray(), prePadBits);
    }

    [Fact]
    public void FromBitReader_Reads_From_Beginning_Of_Buffer()
    {
        // Arrange: no pre-pad, some post-pad to ensure buffer can have extra bits
        var (buffer, _) = BuildBuffer(prePadBits: 0, postPadBits: 7);
        var reader = new BitReader(buffer);

        // Act
        var parsed = DescriptionHashTaggedField.FromBitReader(reader, TaggedFieldConstants.HashLength);

        // Assert
        Assert.Equal(s_knownHash, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Unaligned()
    {
        // Arrange: start the field at a non-byte-aligned offset to stress bit shifting
        var (buffer, fieldOffsetBits) = BuildBuffer(prePadBits: 13, postPadBits: 11);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        // Act
        var parsed = DescriptionHashTaggedField.FromBitReader(reader, TaggedFieldConstants.HashLength);

        // Assert
        Assert.Equal(s_knownHash, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_From_Middle_Of_Buffer_Aligned()
    {
        // Arrange: start the field at a byte-aligned offset in the middle of the buffer
        var (buffer, fieldOffsetBits) = BuildBuffer(prePadBits: 16, postPadBits: 11);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        // Act
        var parsed = DescriptionHashTaggedField.FromBitReader(reader, TaggedFieldConstants.HashLength);

        // Assert
        Assert.Equal(s_knownHash, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Unaligned()
    {
        // Arrange: leave a few bits after the field
        var (buffer, fieldOffsetBits) = BuildBuffer(prePadBits: 5, postPadBits: 3);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        // Act
        var parsed = DescriptionHashTaggedField.FromBitReader(reader, TaggedFieldConstants.HashLength);

        // Assert
        Assert.Equal(s_knownHash, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_At_End_Of_Buffer()
    {
        // Arrange: the field ends exactly at the end of the buffer
        var (buffer, fieldOffsetBits) = BuildBuffer(prePadBits: 7, postPadBits: 0);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        // Act
        var parsed = DescriptionHashTaggedField.FromBitReader(reader, TaggedFieldConstants.HashLength);

        // Assert
        Assert.Equal(s_knownHash, parsed.Value);
    }

    [Fact]
    public void FromBitReader_Reads_Near_End_Of_Buffer_Aligned()
    {
        // Arrange: aligned start with only a few bits after the field
        var (buffer, fieldOffsetBits) = BuildBuffer(prePadBits: 8, postPadBits: 3);
        var reader = new BitReader(buffer);
        reader.SkipBits(fieldOffsetBits);

        // Act
        var parsed = DescriptionHashTaggedField.FromBitReader(reader, TaggedFieldConstants.HashLength);

        // Assert
        Assert.Equal(s_knownHash, parsed.Value);
    }
}