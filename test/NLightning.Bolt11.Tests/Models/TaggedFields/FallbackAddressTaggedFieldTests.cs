using NBitcoin;

namespace NLightning.Bolt11.Tests.Models.TaggedFields;

using Bolt11.Models.TaggedFields;
using Domain.Protocol.ValueObjects;
using Domain.Utils;
using Enums;

public class FallbackAddressTaggedFieldTests
{
    [Theory]
    [InlineData("1RustyRX2oai4EYYDpQGWvEL62BBGqN9T", 33)] // P2PKH
    [InlineData("3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX", 33)] // P2SH
    [InlineData("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", 33)] // P2WPKH
    [InlineData("bc1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3qccfmv3", 53)] // P2WSH
    public void Constructor_FromValue_SetsPropertiesCorrectly(string address, short expectedLength)
    {
        // Arrange
        var network = Network.Main;
        var bitcoinAddress = BitcoinAddress.Create(address, network);

        // Act
        var taggedField = new FallbackAddressTaggedField(bitcoinAddress);

        // Assert
        Assert.Equal(TaggedFieldTypes.FallbackAddress, taggedField.Type);
        Assert.Equal(bitcoinAddress, taggedField.Value);
        Assert.Equal(expectedLength, taggedField.Length);
    }

    [Theory]
    [InlineData("1RustyRX2oai4EYYDpQGWvEL62BBGqN9T",
                new byte[]
                {
                    0x88, 0x25, 0xB0, 0xFB, 0xEE, 0x0F, 0x50, 0x6E,
                    0x4C, 0xA1, 0x22, 0x32, 0x66, 0x20, 0x32, 0x6E,
                    0x2B, 0x26, 0xC8, 0xF4, 0x48
                })] // P2PKH
    [InlineData("3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX",
                new byte[]
                {
                    0x94, 0x7A, 0xAA, 0xB1, 0xDC, 0xD0, 0xCF, 0x99,
                    0x0E, 0x10, 0x8F, 0x4D, 0xCF, 0x9C, 0x66, 0xFB,
                    0x43, 0x75, 0x03, 0xC2, 0x28
                })] // P2SH
    [InlineData("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4",
                new byte[]
                {
                    0x03, 0xA8, 0xF3, 0xB7, 0x40, 0xCC, 0x8C, 0xB6,
                    0xA2, 0xA4, 0xA0, 0xE2, 0x2E, 0x8D, 0x9D, 0x19,
                    0x1F, 0x8A, 0x19, 0xDE, 0xB0
                })] // P2WPKH
    [InlineData("bc1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3qccfmv3",
                new byte[]
                {
                    0x00, 0xC3, 0x18, 0xA1, 0xE0, 0xA6, 0x28, 0xB3,
                    0x40, 0x25, 0xE8, 0xC9, 0x01, 0x9A, 0xB6, 0xD0,
                    0x9B, 0x64, 0xC2, 0xB3, 0xC6, 0x6A, 0x69, 0x3D,
                    0x0D, 0xC6, 0x31, 0x94, 0xB0, 0x24, 0x81, 0x93,
                    0x10, 0x00
                })] // P2WSH
    public void WriteToBitWriter_WritesCorrectData(string address, byte[] expectedData)
    {
        // Arrange
        var network = Network.Main;
        var bitcoinAddress = BitcoinAddress.Create(address, network);
        var taggedField = new FallbackAddressTaggedField(bitcoinAddress);
        var bitWriter = new BitWriter(taggedField.Length * 5);

        // Act
        taggedField.WriteToBitWriter(bitWriter);

        // Assert
        var result = bitWriter.ToArray();

        Assert.Equal(expectedData, result);
    }

    [Theory]
    [InlineData("1RustyRX2oai4EYYDpQGWvEL62BBGqN9T", 33,
                new byte[]
                {
                    0x88, 0x25, 0xB0, 0xFB, 0xEE, 0x0F, 0x50, 0x6E,
                    0x4C, 0xA1, 0x22, 0x32, 0x66, 0x20, 0x32, 0x6E,
                    0x2B, 0x26, 0xC8, 0xF4, 0x48
                })] // P2PKH
    [InlineData("3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX", 33,
                new byte[]
                {
                    0x94, 0x7A, 0xAA, 0xB1, 0xDC, 0xD0, 0xCF, 0x99,
                    0x0E, 0x10, 0x8F, 0x4D, 0xCF, 0x9C, 0x66, 0xFB,
                    0x43, 0x75, 0x03, 0xC2, 0x28
                })] // P2SH
    [InlineData("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", 33,
                new byte[]
                {
                    0x03, 0xA8, 0xF3, 0xB7, 0x40, 0xCC, 0x8C, 0xB6,
                    0xA2, 0xA4, 0xA0, 0xE2, 0x2E, 0x8D, 0x9D, 0x19,
                    0x1F, 0x8A, 0x19, 0xDE, 0xB0
                })] // P2WPKH
    [InlineData("bc1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3qccfmv3", 53,
                new byte[]
                {
                    0x00, 0xC3, 0x18, 0xA1, 0xE0, 0xA6, 0x28, 0xB3,
                    0x40, 0x25, 0xE8, 0xC9, 0x01, 0x9A, 0xB6, 0xD0,
                    0x9B, 0x64, 0xC2, 0xB3, 0xC6, 0x6A, 0x69, 0x3D,
                    0x0D, 0xC6, 0x31, 0x94, 0xB0, 0x24, 0x81, 0x93,
                    0x10, 0x00
                })] // P2WSH
    public void FromBitReader_CreatesCorrectlyFromBitReader(string expectedAddress, short bitLength, byte[] bytes)
    {
        // Arrange
        var bitReader = new BitReader(bytes);

        // Act
        var taggedField = FallbackAddressTaggedField.FromBitReader(bitReader, bitLength, BitcoinNetwork.Mainnet);

        // Assert
        Assert.NotNull(taggedField);
        Assert.Equal(expectedAddress, taggedField.Value.ToString());
    }

    [Fact]
    public void IsValid_ReturnsTrue()
    {
        // Arrange
        var network = Network.Main;
        var address = BitcoinAddress.Create("1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa", network);
        var taggedField = new FallbackAddressTaggedField(address);

        // Act & Assert
        Assert.True(taggedField.IsValid());
    }

    [Theory]
    [InlineData(1)]  // Witness v1 (Taproot) - not yet supported
    [InlineData(2)]  // Future witness version
    [InlineData(16)] // Future witness version
    [InlineData(19)] // Reserved per BOLT 11
    [InlineData(20)] // Reserved per BOLT 11
    [InlineData(31)] // Reserved per BOLT 11 (max 5-bit value)
    public void FromBitReader_ReturnsNull_ForUnknownVersion(byte version)
    {
        // Arrange - Create data with the version in the first 5 bits
        // Version is stored in 5 bits, followed by 20 bytes (160 bits) of address data
        // Total: 5 + 160 = 165 bits = 33 5-bit units
        var data = new byte[21]; // (165 bits + 7) / 8 = 21 bytes
        data[0] = (byte)(version << 3); // Version in first 5 bits (shifted to align)
        // Fill remaining with dummy address data
        for (var i = 1; i < data.Length; i++)
            data[i] = 0xAB;

        var bitReader = new BitReader(data);

        // Act
        var result = FallbackAddressTaggedField.FromBitReader(bitReader, 33, BitcoinNetwork.Mainnet);

        // Assert - Per BOLT 11: "MUST skip over `f` fields that use an unknown `version`"
        Assert.Null(result);
    }

    [Fact]
    public void FromBitReader_ReturnsNull_ForVersion0WithInvalidDataLength()
    {
        // Arrange - Version 0 but with 15-byte data (not 20 or 32)
        // This should return null since it's not a valid P2WPKH (20) or P2WSH (32)
        var data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };
        var bitReader = new BitReader(data);

        // Act
        var result = FallbackAddressTaggedField.FromBitReader(bitReader, 13, BitcoinNetwork.Mainnet);

        // Assert
        Assert.Null(result);
    }
}