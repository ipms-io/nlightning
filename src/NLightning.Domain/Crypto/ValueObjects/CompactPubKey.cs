using NLightning.Domain.Crypto.Constants;

namespace NLightning.Domain.Crypto.ValueObjects;

public readonly record struct CompactPubKey
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
}