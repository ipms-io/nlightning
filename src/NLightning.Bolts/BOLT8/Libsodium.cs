using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NLightning.Bolts.BOLT8;

internal static partial class Libsodium
{
    private const string Name = "libsodium";

    public const int Crypto_scalarmult_curve25519_BYTES = 32;
    public const int Crypto_scalarmult_curve25519_SCALARBYTES = 32;

    static Libsodium()
    {
        if (sodium_init() == -1)
        {
            throw new CryptographicException("Failed to initialize libsodium.");
        }
    }

    [LibraryImport(Name)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int sodium_init();

    [LibraryImport(Name)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int crypto_aead_chacha20poly1305_ietf_encrypt(
        ref byte c,
        out long clen_p,
        ref byte m,
        long mlen,
        ref byte ad,
        long adlen,
        IntPtr nsec,
        ref byte npub,
        ref byte k
    );

    [LibraryImport(Name)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int crypto_aead_chacha20poly1305_ietf_decrypt(
        ref byte m,
        out long mlen_p,
        IntPtr nsec,
        ref byte c,
        long clen,
        ref byte ad,
        long adlen,
        ref byte npub,
        ref byte k
    );

    [LibraryImport(Name)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int crypto_hash_sha256_init(
        IntPtr state
    );

    [LibraryImport(Name)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int crypto_hash_sha256_update(
        IntPtr state,
        ref byte @in,
        ulong inlen
    );

    [LibraryImport(Name)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int crypto_hash_sha256_final(
        IntPtr state,
        ref byte @out
    );
}