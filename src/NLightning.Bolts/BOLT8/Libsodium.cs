using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NLightning.Bolts.BOLT8;

internal static partial class Libsodium
{
    private const string NAME = "libsodium";

    static Libsodium()
    {
        if (sodium_init() == -1)
        {
            throw new CryptographicException("Failed to initialize libsodium.");
        }
    }

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int sodium_init();

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int crypto_aead_chacha20poly1305_ietf_encrypt(
        ref byte c,
        out long clenP,
        ref byte m,
        long mLen,
        ref byte ad,
        long adLen,
        IntPtr nSec,
        ref byte nPub,
        ref byte k
    );

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int crypto_aead_chacha20poly1305_ietf_decrypt(
        ref byte m,
        out long mLenP,
        IntPtr nSec,
        ref byte c,
        long clen,
        ref byte ad,
        long adLen,
        ref byte nPub,
        ref byte k
    );

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int crypto_hash_sha256_init(
        IntPtr state
    );

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int crypto_hash_sha256_update(
        IntPtr state,
        ref byte @in,
        ulong inLen
    );

    [LibraryImport(NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int crypto_hash_sha256_final(
        IntPtr state,
        ref byte @out
    );
}