using System.Diagnostics.CodeAnalysis;

namespace NLightning.Domain.Crypto.Constants;

[ExcludeFromCodeCoverage]
public static class CryptoConstants
{
    public const int Chacha20Poly1305TagLen = 16;
    public const int Xchacha20Poly1305TagLen = 16;

    public const int Chacha20Poly1305NonceLen = 12;
    public const int Xchacha20Poly1305NonceLen = 24;

    public const int Sha256HashLen = 32;
    public const int Sha256BlockLen = 64;
    public const int LibsodiumSha256StateLen = 104;
    
    public const int Ripemd160HashLen = 20;

    public const int ExtPrivkeyLen = 74;
    public const int PrivkeyLen = 32;
    public const int CompactPubkeyLen = 33;
    public const int SecretLen = 32;

    public const ulong FirstPerCommitmentIndex = 281474976710655;

    public const int MaxSignatureSize = 64;
    public const int MinSignatureSize = 63;
}