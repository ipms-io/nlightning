namespace NLightning.Bolts.Tests.BOLT2.Factories;

using Bolts.BOLT2.Factories;
using Common.Crypto.Hashes;

public class ChannelIdFactoryTests
{
    [Fact]
    public void Given_ValidInputs_When_CreatingV1_Then_ReturnsCorrectChannelId()
    {
        // Arrange
        var fundingTxId = new byte[32];
        new Random().NextBytes(fundingTxId);
        const ushort FUNDING_OUTPUT_INDEX = 1;

        // Act
        var channelId = ChannelIdFactory.CreateV1(fundingTxId, FUNDING_OUTPUT_INDEX);

        // Assert
        var expectedChannelId = new byte[32];
        fundingTxId.CopyTo(expectedChannelId, 0);
        expectedChannelId[30] ^= 0;
        expectedChannelId[31] ^= FUNDING_OUTPUT_INDEX & 0xFF;

        Assert.Equal(expectedChannelId, channelId);
    }

    [Fact]
    public void Given_InvalidFundingTxIdLength_When_CreatingV1_Then_ThrowsArgumentException()
    {
        // Arrange
        var fundingTxId = new byte[16]; // Invalid length
        const ushort FUNDING_OUTPUT_INDEX = 1;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ChannelIdFactory.CreateV1(fundingTxId, FUNDING_OUTPUT_INDEX));
        Assert.Equal("Funding transaction ID must be 32 bytes (Parameter 'fundingTxId')", ex.Message);
    }

    [Fact]
    public void Given_ValidInputs_When_CreatingV2_Then_ReturnsCorrectChannelId()
    {
        // Arrange
        var lesserRevocationBasepoint = new byte[33];
        var greaterRevocationBasepoint = new byte[33];
        new Random().NextBytes(lesserRevocationBasepoint);
        new Random().NextBytes(greaterRevocationBasepoint);

        // Act
        var channelId = ChannelIdFactory.CreateV2(lesserRevocationBasepoint, greaterRevocationBasepoint);

        // Assert
        var combined = new byte[66];
        lesserRevocationBasepoint.CopyTo(combined, 0);
        greaterRevocationBasepoint.CopyTo(combined, 33);

        using var sha256 = new Sha256();
        sha256.AppendData(combined);
        var expectedHash = new byte[32];
        sha256.GetHashAndReset(expectedHash);

        Assert.Equal(expectedHash, channelId);
    }

    [Fact]
    public void Given_InvalidLesserRevocationBasepointLength_When_CreatingV2_Then_ThrowsArgumentException()
    {
        // Arrange
        var lesserRevocationBasepoint = new byte[32]; // Invalid length
        var greaterRevocationBasepoint = new byte[33];

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ChannelIdFactory.CreateV2(lesserRevocationBasepoint, greaterRevocationBasepoint));
        Assert.Equal("Revocation basepoints must be 33 bytes each", ex.Message);
    }

    [Fact]
    public void Given_InvalidGreaterRevocationBasepointLength_When_CreatingV2_Then_ThrowsArgumentException()
    {
        // Arrange
        var lesserRevocationBasepoint = new byte[33];
        var greaterRevocationBasepoint = new byte[32]; // Invalid length

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ChannelIdFactory.CreateV2(lesserRevocationBasepoint, greaterRevocationBasepoint));
        Assert.Equal("Revocation basepoints must be 33 bytes each", ex.Message);
    }
}