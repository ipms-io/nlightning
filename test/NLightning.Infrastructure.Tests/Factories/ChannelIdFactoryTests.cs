using NLightning.Infrastructure.Protocol.Factories;

namespace NLightning.Infrastructure.Tests.Factories;

using Infrastructure.Crypto.Hashes;

public class ChannelIdFactoryTests
{
    private readonly ChannelIdFactory _channelIdFactory;

    public ChannelIdFactoryTests()
    {
        _channelIdFactory = new ChannelIdFactory();
    }

    [Fact]
    public void Given_ValidInputs_When_CreatingV2_Then_ReturnsCorrectChannelId()
    {
        // Arrange
        var lesserRevocationBasepoint = new byte[33];
        var greaterRevocationBasepoint = new byte[33];
        lesserRevocationBasepoint[0] = 0x02; // Ensure it's a compressed public key
        greaterRevocationBasepoint[0] = 0x03; // Ensure it's a compressed public key
        new Random().NextBytes(lesserRevocationBasepoint[1..]);
        new Random().NextBytes(greaterRevocationBasepoint[1..]);

        // Act
        var channelId = _channelIdFactory.CreateV2(lesserRevocationBasepoint, greaterRevocationBasepoint);

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
}