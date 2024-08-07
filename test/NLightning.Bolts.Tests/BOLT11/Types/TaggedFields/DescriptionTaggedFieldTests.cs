using System.Text;

namespace NLightning.Bolts.Tests.BOLT11.Types.TaggedFields;

using Bolts.BOLT11.Enums;
using Bolts.BOLT11.Types.TaggedFields;
using Common.BitUtils;

public class DescriptionTaggedFieldTests
{
    private const string EXPECTED_VALUE = "Test Description";
    private readonly byte[] _expectedBytes =
    [
        0x54,
        0x65,
        0x73,
        0x74,
        0x20,
        0x44,
        0x65,
        0x73,
        0x63,
        0x72,
        0x69,
        0x70,
        0x74,
        0x69,
        0x6F,
        0x6E,
        0x00
    ];

    [Fact]
    public void Constructor_FromValue_SetsPropertiesCorrectly()
    {
        // Act
        var taggedField = new DescriptionTaggedField(EXPECTED_VALUE);

        // Assert
        Assert.Equal(TaggedFieldTypes.DESCRIPTION, taggedField.Type);
        Assert.Equal(EXPECTED_VALUE, taggedField.Value);
        Assert.Equal((short)((Encoding.UTF8.GetByteCount(EXPECTED_VALUE) * 8 + 4) / 5), taggedField.Length);
    }

    [Fact]
    public void WriteToBitWriter_WritesCorrectData()
    {
        // Arrange
        var taggedField = new DescriptionTaggedField(EXPECTED_VALUE);
        using var bitWriter = new BitWriter(130);

        // Act
        taggedField.WriteToBitWriter(bitWriter);

        // Assert
        var result = bitWriter.ToArray();

        Assert.Equal(_expectedBytes, result);
    }

    [Fact]
    public void IsValid_ReturnsTrueForNonEmptyValue()
    {
        // Arrange
        var taggedField = new DescriptionTaggedField(EXPECTED_VALUE);

        // Act & Assert
        Assert.True(taggedField.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalseForEmptyValue()
    {
        // Arrange
        var taggedField = new DescriptionTaggedField("");

        // Act & Assert
        Assert.False(taggedField.IsValid());
    }

    [Fact]
    public void FromBitReader_CreatesCorrectlyFromBitReader()
    {
        // Arrange
        using var bitReader = new BitReader(_expectedBytes);

        // Act
        var taggedField = DescriptionTaggedField.FromBitReader(bitReader, 26);

        // Assert
        Assert.Equal(EXPECTED_VALUE, taggedField.Value);
    }

    [Fact]
    public void FromBitReader_ThrowsArgumentException_ForInvalidLength()
    {
        // Arrange
        var buffer = new byte[50];
        using var bitReader = new BitReader(buffer);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DescriptionTaggedField.FromBitReader(bitReader, 0));
    }
}