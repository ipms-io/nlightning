using NBitcoin;

namespace NLightning.Bolt11.Tests.Models.TaggedFields;

using Constants;
using Bolt11.Models.TaggedFields;
using Domain.Utils;
using Enums;

public class PayeePubKeyTaggedFieldTests
{
    private static readonly PubKey s_pubKey = new(
        "03e7156ae33b0a208d0744199163177e909e80176e55d97a2f221ede0f934dd9ad");

    // For WriteToBitWriter we expect 53*5 = 265 bits -> 34 bytes buffer.
    // The last byte is padding that should be zero.
    private static byte[] BuildExpectedBytes()
    {
        var pk = s_pubKey.ToBytes();
        using var writer = new BitWriter(TaggedFieldConstants.PayeePubkeyLength * 5);
        // Use production writer to form the expected 265-bit layout
        writer.WriteBits(pk, TaggedFieldConstants.PayeePubkeyLength * 5);
        return writer.ToArray();
    }

    [Fact]
    public void Constructor_FromValue_SetsPropertiesCorrectly()
    {
        // Act
        var taggedField = new PayeePubKeyTaggedField(s_pubKey);

        // Assert
        Assert.Equal(TaggedFieldTypes.PayeePubKey, taggedField.Type);
        Assert.Equal(s_pubKey.ToHex(), taggedField.Value.ToHex());
        Assert.Equal(TaggedFieldConstants.PayeePubkeyLength, taggedField.Length);
    }

    [Fact]
    public void WriteToBitWriter_WritesCorrectData_WithPadding()
    {
        // Arrange
        var taggedField = new PayeePubKeyTaggedField(s_pubKey);
        var bitWriter = new BitWriter(taggedField.Length * 5);
        var expected = BuildExpectedBytes();

        // Act
        taggedField.WriteToBitWriter(bitWriter);

        // Assert
        var result = bitWriter.ToArray();
        Assert.Equal(expected, result);
        // Ensure the padding byte is present and zero
        Assert.Equal(34, result.Length);
        Assert.Equal(0x00, result[^1]);
    }

    [Fact]
    public void IsValid_ReturnsTrue()
    {
        // Arrange
        var taggedField = new PayeePubKeyTaggedField(s_pubKey);

        // Act & Assert
        Assert.True(taggedField.IsValid());
    }

    [Fact]
    public void FromBitReader_CreatesCorrectlyFromBitReader()
    {
        // Arrange
        var expected = s_pubKey.ToHex();
        var buffer = BuildExpectedBytes();
        var reader = new BitReader(buffer);

        // Act
        var taggedField = PayeePubKeyTaggedField.FromBitReader(reader, TaggedFieldConstants.PayeePubkeyLength);

        // Assert
        Assert.Equal(expected, taggedField.Value.ToHex());
    }
}
