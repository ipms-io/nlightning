using System.Diagnostics.CodeAnalysis;

namespace NLightning.Domain.Crypto.Constants;

[ExcludeFromCodeCoverage]
public static class CryptoConstants
{
    public const int CHACHA20_POLY1305_TAG_LEN = 16;
    public const int XCHACHA20_POLY1305_TAG_LEN = 16;

    public const int CHACHA20_POLY1305_NONCE_LEN = 12;
    public const int XCHACHA20_POLY1305_NONCE_LEN = 24;

    public const int SHA256_HASH_LEN = 32;
    public const int SHA256_BLOCK_LEN = 64;
    public const int LIBSODIUM_SHA256_STATE_LEN = 104;

    public const int LIBSODIUM_RIPEMD160_STATE_LEN = 80;

    public const int PRIVKEY_LEN = 32;
    public const int PUBKEY_LEN = 33;

    public const int MAX_SIGNATURE_SIZE = 64;
}