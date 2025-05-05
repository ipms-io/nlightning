#if CRYPTO_NATIVE
namespace NLightning.Common.Tests.Crypto.Providers.Native;

using Common.Crypto.Providers.Native;
using Vectors;

public class NativeCryptoProviderTests
{
    [Fact]
    public void GivenValidInputs_WhenAeadChacha20Poly1305IetfEncryptCalled_ThenCallsNativeCryptoAeadChacha20Poly1305IetfEncrypt()
    {
        // Arrange
        var cryptoProvider = new NativeCryptoProvider();
        Span<byte> cipher = new byte[AeadChacha20Poly1305IetfVector.MESSAGE.Length + 16];
        
        // Act
        cryptoProvider
            .AeadChaCha20Poly1305IetfEncrypt(AeadChacha20Poly1305IetfVector.KEY, 
                                             AeadChacha20Poly1305IetfVector.PUBLIC_NONCE, null,
                                             AeadChacha20Poly1305IetfVector.AUTHENTICATION_DATA,
                                             AeadChacha20Poly1305IetfVector.MESSAGE, cipher, out var clenP);

        // Assert 
        Assert.Equal(AeadChacha20Poly1305IetfVector.CIPHER.Length, clenP);
        Assert.Equal(AeadChacha20Poly1305IetfVector.CIPHER, cipher.ToArray());
    }
    
    
    [Fact]
    public void GivenValidInputs_WhenAeadChacha20Poly1305IetfDecryptCalled_ThenCallsNativeCryptoAeadChacha20Poly1305IetfDecrypt()
    {
        // Arrange
        var cryptoProvider = new NativeCryptoProvider();
        Span<byte> message = new byte[AeadChacha20Poly1305IetfVector.MESSAGE.Length];
        
        // Act
        cryptoProvider
            .AeadChaCha20Poly1305IetfDecrypt(AeadChacha20Poly1305IetfVector.KEY, 
                AeadChacha20Poly1305IetfVector.PUBLIC_NONCE, null,
                AeadChacha20Poly1305IetfVector.AUTHENTICATION_DATA,
                AeadChacha20Poly1305IetfVector.CIPHER, message, out var clenP);

        // Assert 
        Assert.Equal(AeadChacha20Poly1305IetfVector.MESSAGE.Length, clenP);
        Assert.Equal(AeadChacha20Poly1305IetfVector.MESSAGE, message.ToArray());
    }
    
    [Fact]
    public void GivenValidInputs_WhenAeadXChacha20Poly1305IetfEncryptCalled_ThenCallsNativeCryptoAeadChacha20Poly1305IetfEncrypt()
    {
        // Arrange
        var cryptoProvider = new NativeCryptoProvider();
        Span<byte> cipher = new byte[AeadXChacha20Poly1305IetfVector.MESSAGE.Length + 16];
        
        // Act
        cryptoProvider
            .AeadXChaCha20Poly1305IetfEncrypt(AeadXChacha20Poly1305IetfVector.KEY,
                                              AeadXChacha20Poly1305IetfVector.PUBLIC_NONCE,
                                              AeadXChacha20Poly1305IetfVector.AUTHENTICATION_DATA,
                                              AeadXChacha20Poly1305IetfVector.MESSAGE, cipher, out var clenP);

        // Assert 
        Assert.Equal(AeadXChacha20Poly1305IetfVector.CIPHER.Length, clenP);
        Assert.Equal(AeadXChacha20Poly1305IetfVector.CIPHER, cipher.ToArray());
    }
    
    [Fact]
    public void GivenValidInputs_WhenAeadXChacha20Poly1305IetfDecryptCalled_ThenCallsNativeCryptoAeadChacha20Poly1305IetfDecrypt()
    {
        // Arrange
        var cryptoProvider = new NativeCryptoProvider();
        Span<byte> message = new byte[AeadXChacha20Poly1305IetfVector.MESSAGE.Length];
        
        // Act
        cryptoProvider
            .AeadXChaCha20Poly1305IetfDecrypt(AeadXChacha20Poly1305IetfVector.KEY,
                                              AeadXChacha20Poly1305IetfVector.PUBLIC_NONCE,
                                              AeadXChacha20Poly1305IetfVector.AUTHENTICATION_DATA,
                                              AeadXChacha20Poly1305IetfVector.CIPHER, message, out var clenP);

        // Assert 
        Assert.Equal(AeadXChacha20Poly1305IetfVector.MESSAGE.Length, clenP);
        Assert.Equal(AeadXChacha20Poly1305IetfVector.MESSAGE, message.ToArray());
    }
}
#endif