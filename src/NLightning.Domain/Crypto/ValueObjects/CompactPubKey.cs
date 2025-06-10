namespace NLightning.Domain.Crypto.ValueObjects;

using Constants;
using Utils.Extensions;

public readonly struct CompactPubKey : IEquatable<CompactPubKey>
{
    private readonly byte[] _compactBytes;

    public CompactPubKey(byte[] compactBytes)
    {
        if (compactBytes.Length != CryptoConstants.CompactPubkeyLen)
            throw new ArgumentException("PublicKey cannot be empty.", nameof(compactBytes));

        if (compactBytes[0] != 0x02 && compactBytes[0] != 0x03)
            throw new ArgumentException("Invalid CompactPubKey format. The first byte must be 0x02 or 0x03.",
                                        nameof(compactBytes));

        _compactBytes = compactBytes;
    }

    public static implicit operator CompactPubKey(byte[] bytes) => new(bytes);
    public static implicit operator byte[](CompactPubKey hash) => hash._compactBytes;

    public static implicit operator ReadOnlySpan<byte>(CompactPubKey compactPubKey) => compactPubKey._compactBytes;

    public static implicit operator ReadOnlyMemory<byte>(CompactPubKey compactPubKey) => compactPubKey._compactBytes;

    public static bool operator !=(CompactPubKey left, CompactPubKey right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(CompactPubKey left, CompactPubKey right)
    {
        return left.Equals(right);
    }

    public override string ToString() => Convert.ToHexString(_compactBytes).ToLowerInvariant();

    public bool Equals(CompactPubKey other)
    {
        return _compactBytes.SequenceEqual(other._compactBytes);
    }

    public override bool Equals(object? obj)
    {
        return obj is CompactPubKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _compactBytes.GetByteArrayHashCode();
    }
}