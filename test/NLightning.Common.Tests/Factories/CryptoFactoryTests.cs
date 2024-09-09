namespace NLightning.Common.Tests.Factories;

public class CryptoFactoryTests
{
#if BLAZOR_WEBASSEMBLY
    [Fact]
    public void Given_WEBASSEMBLYDirective_When_GetSodiumCryptoCalled_Then_ReturnsSodiumCryptoJs()
    {
        // Given
        // BLAZOR_WEBASSEMBLY directive is given by build configuration

        // When
        var result = CryptoFactory.GetCryptoProvider();

        // Then
        Assert.IsType<SodiumJsCryptoProvider>(result);
    }
#else
    [Fact]
    public void Given_NoWEBASSEMBLYDirective_When_GetSodiumCryptoCalled_Then_ReturnsSodiumCryptoNative()
    {
        // Given
        // No BLAZOR_WEBASSEMBLY directive is given by build configuration

        // When
        // var result = SodiumCryptoFactory.GetSodiumCrypto();
        //
        // // Then
        // Assert.IsType<SodiumCryptoNative>(result);
    }
#endif
}