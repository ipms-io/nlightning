namespace NLightning.Bolts.Tests.BOLT11.Types.TaggedFields;

using Bolts.BOLT11.Enums;
using Bolts.BOLT11.Types.TaggedFields;
using Common.BitUtils;

public class MetadataTaggedFieldTests
{
    [Theory]
    [InlineData(new byte[] { 1 }, 2)]
    [InlineData(new byte[] { 1, 2 }, 4)]
    [InlineData(new byte[] { 1, 2, 3 }, 5)]
    [InlineData(new byte[] { 1, 2, 3, 4 }, 7)]
    [InlineData(new byte[] { 1, 2, 3, 4, 5 }, 8)]
    public void Constructor_FromValue_SetsPropertiesCorrectly(byte[] metadata, short expectedLength)
    {
        // Act
        var taggedField = new MetadataTaggedField(metadata);

        // Assert
        Assert.Equal(TaggedFieldTypes.METADATA, taggedField.Type);
        Assert.Equal(metadata, taggedField.Value);
        Assert.Equal(expectedLength, taggedField.Length);
    }

    [Theory]
    [InlineData(new byte[] { 1 }, new byte[] { 1, 0 })]
    [InlineData(new byte[] { 1, 2 }, new byte[] { 1, 2, 0 })]
    [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3, 0 })]
    [InlineData(new byte[] { 1, 2, 3, 4 }, new byte[] { 1, 2, 3, 4, 0 })]
    [InlineData(new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 1, 2, 3, 4, 5 })]
    public void WriteToBitWriter_WritesCorrectData(byte[] metadata, byte[] expectedData)
    {
        // Arrange
        var taggedField = new MetadataTaggedField(metadata);
        using var bitWriter = new BitWriter(taggedField.Length * 5);

        // Act
        taggedField.WriteToBitWriter(bitWriter);

        // Assert
        var result = bitWriter.ToArray();

        Assert.Equal(expectedData, result);
    }

    [Theory]
    [InlineData(new byte[] { 1 }, 2, new byte[] { 1, 0 })]
    [InlineData(new byte[] { 1, 2 }, 4, new byte[] { 1, 2, 0 })]
    [InlineData(new byte[] { 1, 2, 3 }, 5, new byte[] { 1, 2, 3, 0 })]
    [InlineData(new byte[] { 1, 2, 3, 4 }, 7, new byte[] { 1, 2, 3, 4, 0 })]
    [InlineData(new byte[] { 1, 2, 3, 4, 5 }, 8, new byte[] { 1, 2, 3, 4, 5 })]
    public void FromBitReader_CreatesCorrectlyFromBitReader(byte[] expectedMetadata, short bitLength, byte[] bytes)
    {
        // Arrange
        using var bitReader = new BitReader(bytes);

        // Act
        var taggedField = MetadataTaggedField.FromBitReader(bitReader, bitLength);

        // Assert
        Assert.Equal(expectedMetadata, taggedField.Value);
    }

    [Fact]
    public void FromBitReader_ThrowsArgumentException_ForInvalidLength()
    {
        // Arrange
        var buffer = new byte[50];
        using var bitReader = new BitReader(buffer);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => MetadataTaggedField.FromBitReader(bitReader, 0));
    }
}