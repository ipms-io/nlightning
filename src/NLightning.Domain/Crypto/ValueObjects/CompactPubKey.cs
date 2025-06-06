using NLightning.Domain.Crypto.Constants;
using NLightning.Domain.Utils.Extensions;

namespace NLightning.Domain.Crypto.ValueObjects;

public readonly struct CompactPubKey : IEquatable<CompactPubKey>
{
    public byte[] CompactBytes { get; }

    public CompactPubKey(byte[] compactBytes)
    {
        if (compactBytes.Length != CryptoConstants.CompactPubkeyLen)
            throw new ArgumentException("PublicKey cannot be empty.", nameof(compactBytes));

        if (compactBytes[0] != 0x02 && compactBytes[0] != 0x03)
            throw new ArgumentException("Invalid CompactPubKey format. The first byte must be 0x02 or 0x03.",
                                        nameof(compactBytes));

        CompactBytes = compactBytes;
    }

    public static implicit operator CompactPubKey(byte[] bytes) => new(bytes);
    public static implicit operator byte[](CompactPubKey hash) => hash.CompactBytes;

    public static implicit operator ReadOnlySpan<byte>(CompactPubKey compactPubKey) => compactPubKey.CompactBytes;
    public static implicit operator ReadOnlyMemory<byte>(CompactPubKey compactPubKey) => compactPubKey.CompactBytes;

    public override string ToString() => Convert.ToHexString(CompactBytes);

    public bool Equals(CompactPubKey other)
    {
        return CompactBytes.SequenceEqual(other.CompactBytes);
    }

    public override bool Equals(object? obj)
    {
        return obj is CompactPubKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return CompactBytes.GetByteArrayHashCode();
    }
}