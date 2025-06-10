#if CRYPTO_NATIVE
using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Crypto.Providers.Native.Constants;

[ExcludeFromCodeCoverage]
public static class XChaCha20Constants
{
    public const int KeySize = 32;
    public const int NonceSize = 24;
    public const int StateSize = 16;
    public const int SubkeySize = 32;
}
#endif