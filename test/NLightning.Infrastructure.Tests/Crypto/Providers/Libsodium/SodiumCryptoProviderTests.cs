#if CRYPTO_LIBSODIUM
namespace NLightning.Infrastructure.Tests.Crypto.Providers.Libsodium;

using Infrastructure.Crypto.Providers.Libsodium;

public class SodiumCryptoProviderTests
{
    [Fact]
    public void GivenValidInputs_WhenAeadChacha20Poly1305IetfEncryptCalled_ThenCallsLibsodiumCryptoAeadChacha20Poly1305IetfEncrypt()
    {
        // Arrange
        var cryptoProvider = new SodiumCryptoProvider();
        Span<byte> cipher = new byte[AeadChacha20Poly1305IetfVector.Message.Length + 16];

        // Act
        cryptoProvider.AeadChaCha20Poly1305IetfEncrypt(AeadChacha20Poly1305IetfVector.Key,
                                                       AeadChacha20Poly1305IetfVector.PublicNonce, null,
                                                       AeadChacha20Poly1305IetfVector.AuthenticationData,
                                                       AeadChacha20Poly1305IetfVector.Message, cipher, out var clenP);

        // Assert 
        Assert.Equal(AeadChacha20Poly1305IetfVector.Cipher.Length, clenP);
        Assert.Equal(AeadChacha20Poly1305IetfVector.Cipher, cipher.ToArray());
    }

    [Fact]
    public void GivenValidInputs_WhenAeadChacha20Poly1305IetfDecryptCalled_ThenCallsLibsodiumCryptoAeadChacha20Poly1305IetfDecrypt()
    {
        // Arrange
        var cryptoProvider = new SodiumCryptoProvider();
        Span<byte> message = new byte[AeadChacha20Poly1305IetfVector.Message.Length];

        // Act
        cryptoProvider.AeadChaCha20Poly1305IetfDecrypt(AeadChacha20Poly1305IetfVector.Key,
                                                       AeadChacha20Poly1305IetfVector.PublicNonce, null,
                                                       AeadChacha20Poly1305IetfVector.AuthenticationData,
                                                       AeadChacha20Poly1305IetfVector.Cipher, message, out var clenP);

        // Assert 
        Assert.Equal(AeadChacha20Poly1305IetfVector.Message.Length, clenP);
        Assert.Equal(AeadChacha20Poly1305IetfVector.Message, message.ToArray());
    }

    [Fact]
    public void GivenValidInputs_WhenAeadXChacha20Poly1305IetfEncryptCalled_ThenCallsLibsodiumCryptoAeadXChacha20Poly1305IetfEncrypt()
    {
        // Arrange
        var cryptoProvider = new SodiumCryptoProvider();
        Span<byte> cipher = new byte[AeadXChacha20Poly1305IetfVector.Message.Length + 16];

        // Act
        cryptoProvider.AeadXChaCha20Poly1305IetfEncrypt(AeadXChacha20Poly1305IetfVector.Key,
                                                        AeadXChacha20Poly1305IetfVector.PublicNonce,
                                                        AeadXChacha20Poly1305IetfVector.AuthenticationData,
                                                        AeadXChacha20Poly1305IetfVector.Message, cipher, out var clenP);

        // Assert 
        Assert.Equal(AeadXChacha20Poly1305IetfVector.Cipher.Length, clenP);
        Assert.Equal(AeadXChacha20Poly1305IetfVector.Cipher, cipher.ToArray());
    }

    [Fact]
    public void GivenValidInputs_WhenAeadXChacha20Poly1305IetfDecryptCalled_ThenCallsLibsodiumCryptoAeadXChacha20Poly1305IetfDecrypt()
    {
        // Arrange
        var cryptoProvider = new SodiumCryptoProvider();
        Span<byte> message = new byte[AeadXChacha20Poly1305IetfVector.Message.Length];

        // Act
        cryptoProvider.AeadXChaCha20Poly1305IetfDecrypt(AeadXChacha20Poly1305IetfVector.Key,
                                                        AeadXChacha20Poly1305IetfVector.PublicNonce,
                                                        AeadXChacha20Poly1305IetfVector.AuthenticationData,
                                                        AeadXChacha20Poly1305IetfVector.Cipher, message, out var clenP);

        // Assert 
        Assert.Equal(AeadXChacha20Poly1305IetfVector.Message.Length, clenP);
        Assert.Equal(AeadXChacha20Poly1305IetfVector.Message, message.ToArray());
    }
}
#endif