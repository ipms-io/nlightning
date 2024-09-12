#if CRYPTO_LIBSODIUM
namespace NLightning.Common.Crypto.Providers.Libsodium;

using System.Runtime.InteropServices;
using System.Security.Cryptography;

internal static partial class LibsodiumWrapper
{
    private const string NAME = "libsodium";

    static LibsodiumWrapper()
    {
        if (sodium_init() == -1)
        {
            throw new CryptographicException("Failed to initialize libsodium.");
        }
    }

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int sodium_init();

    #region SHA256
    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int crypto_hash_sha256_init(IntPtr state);

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int crypto_hash_sha256_update(IntPtr state, ref byte @in, ulong inLen);

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int crypto_hash_sha256_final(IntPtr state, ref byte @out);
    #endregion

    #region AEAD ChaCha20 Poly1305
    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int crypto_aead_chacha20poly1305_ietf_encrypt(ref byte c, out long clenP, ref byte m,
                                                                        long mLen, ref byte ad, long adLen, IntPtr nSec,
                                                                        ref byte nPub, ref byte k);

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int crypto_aead_chacha20poly1305_ietf_decrypt(ref byte m, out long mLenP, IntPtr nSec,
                                                                        ref byte c, long clen, ref byte ad, long adLen,
                                                                        ref byte nPub, ref byte k);
    #endregion

    #region Secure Memory
    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr sodium_malloc(ulong size);

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int sodium_mlock(IntPtr addr, ulong len);

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void sodium_free(IntPtr ptr);

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void sodium_memzero(IntPtr ptr, ulong len);

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void sodium_munlock(IntPtr addr, ulong len);
    #endregion
}
#endif