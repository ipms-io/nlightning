using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT11.Types.TaggedFields;

using Bolts.BOLT11.Enums;
using Bolts.BOLT11.Types.TaggedFields;
using Common.BitUtils;

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
        Assert.Equal(TaggedFieldTypes.FALLBACK_ADDRESS, taggedField.Type);
        Assert.Equal(bitcoinAddress, taggedField.Value);
        Assert.Equal(expectedLength, taggedField.Length);
    }

    [Theory]
    [InlineData("1RustyRX2oai4EYYDpQGWvEL62BBGqN9T", new byte[] { 0x88, 0x25, 0xB0, 0xFB, 0xEE, 0x0F, 0x50, 0x6E, 0x4C, 0xA1, 0x22, 0x32, 0x66, 0x20, 0x32, 0x6E, 0x2B, 0x26, 0xC8, 0xF4, 0x48 })] // P2PKH
    [InlineData("3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX", new byte[] { 0x94, 0x7A, 0xAA, 0xB1, 0xDC, 0xD0, 0xCF, 0x99, 0x0E, 0x10, 0x8F, 0x4D, 0xCF, 0x9C, 0x66, 0xFB, 0x43, 0x75, 0x03, 0xC2, 0x28 })] // P2SH
    [InlineData("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", new byte[] { 0x03, 0xA8, 0xF3, 0xB7, 0x40, 0xCC, 0x8C, 0xB6, 0xA2, 0xA4, 0xA0, 0xE2, 0x2E, 0x8D, 0x9D, 0x19, 0x1F, 0x8A, 0x19, 0xDE, 0xB0 })] // P2WPKH
    [InlineData("bc1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3qccfmv3", new byte[] { 0x00, 0xC3, 0x18, 0xA1, 0xE0, 0xA6, 0x28, 0xB3, 0x40, 0x25, 0xE8, 0xC9, 0x01, 0x9A, 0xB6, 0xD0, 0x9B, 0x64, 0xC2, 0xB3, 0xC6, 0x6A, 0x69, 0x3D, 0x0D, 0xC6, 0x31, 0x94, 0xB0, 0x24, 0x81, 0x93, 0x10, 0x00 })] // P2WSH
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
    [InlineData("1RustyRX2oai4EYYDpQGWvEL62BBGqN9T", 33, new byte[] { 0x88, 0x25, 0xB0, 0xFB, 0xEE, 0x0F, 0x50, 0x6E, 0x4C, 0xA1, 0x22, 0x32, 0x66, 0x20, 0x32, 0x6E, 0x2B, 0x26, 0xC8, 0xF4, 0x48 })] // P2PKH
    [InlineData("3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX", 33, new byte[] { 0x94, 0x7A, 0xAA, 0xB1, 0xDC, 0xD0, 0xCF, 0x99, 0x0E, 0x10, 0x8F, 0x4D, 0xCF, 0x9C, 0x66, 0xFB, 0x43, 0x75, 0x03, 0xC2, 0x28 })] // P2SH
    [InlineData("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", 33, new byte[] { 0x03, 0xA8, 0xF3, 0xB7, 0x40, 0xCC, 0x8C, 0xB6, 0xA2, 0xA4, 0xA0, 0xE2, 0x2E, 0x8D, 0x9D, 0x19, 0x1F, 0x8A, 0x19, 0xDE, 0xB0 })] // P2WPKH
    [InlineData("bc1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3qccfmv3", 53, new byte[] { 0x00, 0xC3, 0x18, 0xA1, 0xE0, 0xA6, 0x28, 0xB3, 0x40, 0x25, 0xE8, 0xC9, 0x01, 0x9A, 0xB6, 0xD0, 0x9B, 0x64, 0xC2, 0xB3, 0xC6, 0x6A, 0x69, 0x3D, 0x0D, 0xC6, 0x31, 0x94, 0xB0, 0x24, 0x81, 0x93, 0x10, 0x00 })] // P2WSH
    public void FromBitReader_CreatesCorrectlyFromBitReader(string expectedAddress, short bitLength, byte[] bytes)
    {
        // Arrange
        var bitReader = new BitReader(bytes);

        // Act
        var taggedField = FallbackAddressTaggedField.FromBitReader(bitReader, bitLength, Network.Main);

        // Assert
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

    [Fact]
    public void FromBitReader_ThrowsArgumentException_ForInvalidAddressType()
    {
        // Arrange
        var invalidData = new byte[] { 255, 0x00, 0x00, 0x00 };
        var bitReader = new BitReader(invalidData);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => FallbackAddressTaggedField.FromBitReader(bitReader, 4, Network.Main));
    }
}