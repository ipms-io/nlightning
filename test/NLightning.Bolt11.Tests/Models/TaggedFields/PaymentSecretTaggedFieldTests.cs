using NBitcoin;

namespace NLightning.Bolt11.Tests.Models.TaggedFields;

using Bolt11.Models.TaggedFields;
using Constants;
using Domain.Utils;
using Enums;

public class PaymentSecretTaggedFieldTests
{
    private readonly byte[] _expectedBytes =
    [
        0x00, 0x01, 0x02, 0x03, 0x04,
        0x05, 0x06, 0x07, 0x08, 0x09,
        0x0A, 0x0B, 0x0C, 0x0D, 0x0E,
        0x0F, 0x10, 0x11, 0x12, 0x13,
        0x14, 0x15, 0x16, 0x17, 0x18,
        0x19, 0x1A, 0x1B, 0x1C, 0x1D,
        0x1E, 0x1F, 0x00
    ];

    private const string SecretString = "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f";

    [Fact]
    public void Constructor_FromValue_SetsPropertiesCorrectly()
    {
        // Arrange
        var expectedValue = new uint256(SecretString);

        // Act
        var taggedField = new PaymentSecretTaggedField(expectedValue);

        // Assert
        Assert.Equal(TaggedFieldTypes.PaymentSecret, taggedField.Type);
        Assert.Equal(expectedValue, taggedField.Value);
        Assert.Equal(TaggedFieldConstants.HashLength, taggedField.Length);
    }

    [Fact]
    public void WriteToBitWriter_WritesCorrectData()
    {
        // Arrange
        var value = new uint256(SecretString);
        var taggedField = new PaymentSecretTaggedField(value);
        var bitWriter = new BitWriter(TaggedFieldConstants.HashLength * 5);

        // Act
        taggedField.WriteToBitWriter(bitWriter);

        // Assert
        var currentBytes = bitWriter.ToArray();
        Assert.Equal(_expectedBytes, currentBytes);
    }

    [Fact]
    public void IsValid_ReturnsTrueForNonZeroValue()
    {
        // Arrange
        var value = new uint256(SecretString);
        var taggedField = new PaymentSecretTaggedField(value);

        // Act & Assert
        Assert.True(taggedField.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalseForZeroValue()
    {
        // Arrange
        var value = uint256.Zero;
        var taggedField = new PaymentSecretTaggedField(value);

        // Act & Assert
        Assert.False(taggedField.IsValid());
    }

    [Fact]
    public void FromBitReader_CreatesCorrectlyFromBitReader()
    {
        // Arrange
        var expectedValue = new uint256(SecretString);
        var bitReader = new BitReader(_expectedBytes);

        // Act
        var taggedField = PaymentSecretTaggedField.FromBitReader(bitReader, TaggedFieldConstants.HashLength);

        // Assert
        Assert.Equal(expectedValue, taggedField.Value);
    }
}