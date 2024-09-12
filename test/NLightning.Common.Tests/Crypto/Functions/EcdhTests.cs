using NLightning.Common.Crypto.Functions;

namespace NLightning.Common.Tests.Crypto.Functions;

public class EcdhTests
{
    [Fact]
    public void Given_InvalidPrivateKeyLength_When_GenerateKeyPairIsCalled_Then_ItThrowsArgumentException()
    {
        // Arrange
        var ecdh = new Ecdh();
        var privateKey = new byte[31];

        // Assert
        var exception = Assert.Throws<ArgumentException>(Act);
        Assert.Equal("Invalid private key length", exception.Message);
        return;

        // Act
        void Act() => ecdh.GenerateKeyPair(privateKey);
    }
}