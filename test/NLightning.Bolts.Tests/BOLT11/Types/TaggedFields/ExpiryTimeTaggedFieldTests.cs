namespace NLightning.Bolts.Tests.BOLT11.Types.TaggedFields;

using Bolts.BOLT11.Enums;
using Bolts.BOLT11.Types.TaggedFields;
using Common.BitUtils;

public class ExpiryTimeTaggedFieldTests
{
    private const int EXPECTED_VALUE = 3600; // 1 hour in seconds
    private readonly byte[] _expectedBytes = [0x1C, 0x20];
    private const short EXPECTED_LENGTH = 3;

    [Fact]
    public void Constructor_FromValue_SetsPropertiesCorrectly()
    {
        // Act
        var taggedField = new ExpiryTimeTaggedField(EXPECTED_VALUE);

        // Assert
        Assert.Equal(TaggedFieldTypes.EXPIRY_TIME, taggedField.Type);
        Assert.Equal(EXPECTED_VALUE, taggedField.Value);
        Assert.Equal(EXPECTED_LENGTH, taggedField.Length);
    }

    [Fact]
    public void WriteToBitWriter_WritesCorrectData()
    {
        // Arrange
        var taggedField = new ExpiryTimeTaggedField(EXPECTED_VALUE);
        using var bitWriter = new BitWriter(taggedField.Length * 5);

        // Act
        taggedField.WriteToBitWriter(bitWriter);

        // Assert
        var result = bitWriter.ToArray();

        Assert.Equal(_expectedBytes, result);
    }

    [Fact]
    public void IsValid_ReturnsTrueForPositiveValue()
    {
        // Arrange
        var taggedField = new ExpiryTimeTaggedField(EXPECTED_VALUE);

        // Act & Assert
        Assert.True(taggedField.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalseForNonPositiveValue()
    {
        //
    }
}