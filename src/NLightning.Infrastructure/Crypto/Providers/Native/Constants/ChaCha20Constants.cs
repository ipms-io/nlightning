#if CRYPTO_NATIVE
using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Crypto.Providers.Native.Constants;

[ExcludeFromCodeCoverage]
public static class XChaCha20Constants
{
    public const int KEY_SIZE = 32;
    public const int NONCE_SIZE = 24;
    public const int STATE_SIZE = 16;
    public const int SUBKEY_SIZE = 32;
}
#endif