namespace NLightning.Infrastructure.Tests.Factories;

using Infrastructure.Crypto.Hashes;
using Infrastructure.Factories;

public class ChannelIdFactoryTests
{
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