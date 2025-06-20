#if CRYPTO_JS
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace NLightning.Infrastructure.Crypto.Providers.JS;

[SupportedOSPlatform("browser")]
internal static partial class LibsodiumJsWrapper
{
    private const string ModuleName = "blazorSodium";
    
    [JSImport("init", "blazorSodium")]
    internal static partial Task InitializeAsync();
    
    #region Sha256
    [JSImport("sodium.libsodium._crypto_hash_sha256_init", ModuleName)]
    internal static partial int crypto_hash_sha256_init(IntPtr state);

    [JSImport("sodium.libsodium._crypto_hash_sha256_update", ModuleName)]
    internal static partial void crypto_hash_sha256_update(IntPtr state, IntPtr data,
                                                           [JSMarshalAs<JSType.Number>]int length);

    [JSImport("sodium.libsodium._crypto_hash_sha256_final", ModuleName)]
    internal static partial void crypto_hash_sha256_final(IntPtr state, IntPtr result);
    #endregion

    #region AEAD ChaCha20 Poly1305
    [JSImport("sodium.crypto_aead_chacha20poly1305_ietf_encrypt", ModuleName)]
    internal static partial byte[] crypto_aead_chacha20poly1305_ietf_encrypt(byte[] message, byte[]? additionalData,
                                                                             byte[]? secretNonce, byte[] publicNonce,
                                                                             byte[] key);

    [JSImport("sodium.crypto_aead_chacha20poly1305_ietf_decrypt", ModuleName)]
    internal static partial byte[] crypto_aead_chacha20poly1305_ietf_decrypt(byte[]? secretNonce, byte[] ciphertext,
                                                                             byte[]? additionalData, byte[] publicNonce,
                                                                             byte[] key);
    #endregion
    
    #region AEAD XChaCha20 Poly1305
    [JSImport("sodium.crypto_aead_xchacha20poly1305_ietf_encrypt", ModuleName)]
    internal static partial byte[] crypto_aead_xchacha20poly1305_ietf_encrypt(byte[] message, byte[]? additionalData,
                                                                              byte[]? secretNonce, byte[] publicNonce,
                                                                              byte[] key);
    
    [JSImport("sodium.crypto_aead_xchacha20poly1305_ietf_decrypt", ModuleName)]
    internal static partial byte[] crypto_aead_xchacha20poly1305_ietf_decrypt(byte[]? secretNonce, byte[] ciphertext,
                                                                              byte[]? additionalData,
                                                                              byte[] publicNonce, byte[] key);
    #endregion
    
    #region Random Bytes
    [JSImport("sodium.randombytes_buf", ModuleName)]
    internal static partial byte[] randombytes_buf(int size);
    #endregion

    #region Secure Memory
    [JSImport("sodium.libsodium._malloc", ModuleName)]
    internal static partial IntPtr sodium_malloc([JSMarshalAs<JSType.Number>] long size);

    [JSImport("sodium.libsodium._free", ModuleName)]
    internal static partial void sodium_free(IntPtr ptr);

    [JSImport("sodium.memzero", ModuleName)]
    internal static partial void sodium_memzero(IntPtr ptr, [JSMarshalAs<JSType.Number>] long len);
    #endregion
    
    #region HeapU8Manipulation
    [JSImport("sodium.libsodium.HEAPU8.set", ModuleName)]
    internal static partial void HEAPU8_set(byte[] src, IntPtr dest);

    [JSImport("sodium.libsodium.HEAPU8.subarray", ModuleName)]
    internal static partial byte[] HEAPU8_subarray(IntPtr ptr, [JSMarshalAs<JSType.Number>] long len);

    #endregion
}
#endif