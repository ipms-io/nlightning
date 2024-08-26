namespace NLightning.Bolts.Tests.BOLT8.Dhs;

using Bolts.BOLT8.Dhs;

public class SecP256K1Tests
{
    [Fact]
    public void Given_InvalidPrivateKeyLength_When_GenerateKeyPairIsCalled_Then_ItThrowsArgumentException()
    {
        // Arrange
        var secp256K1 = new SecP256K1();
        var privateKey = new byte[31];

        // Act
        void Act() => secp256K1.GenerateKeyPair(privateKey);

        // Assert
        var exception = Assert.Throws<ArgumentException>(Act);
        Assert.Equal("Invalid private key length", exception.Message);
    }
}