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
        cryptoProvider.AeadChacha20Poly1305IetfEncrypt(AeadChacha20Poly1305IetfVector.KEY,
            AeadChacha20Poly1305IetfVector.PUBLIC_NONCE, null,
            AeadChacha20Poly1305IetfVector.AUTHENTICATION_DATA,
            AeadChacha20Poly1305IetfVector.MESSAGE, cipher, out var clenP);

        // Assert 
        Assert.Equal(AeadChacha20Poly1305IetfVector.CIPHER.Length, clenP);
        Assert.Equal(AeadChacha20Poly1305IetfVector.CIPHER, cipher.ToArray());
    }
}
#endif