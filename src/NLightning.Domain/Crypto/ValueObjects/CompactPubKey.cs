using NLightning.Domain.Crypto.Constants;

namespace NLightning.Domain.Crypto.ValueObjects;

public readonly record struct CompactPubKey
{
    public byte[] CompactBytes { get; }
    
    public static CompactPubKey Empty => new byte[CryptoConstants.CompactPubkeyLen];

    public CompactPubKey(byte[] compactBytes)
    {
        if (compactBytes.Length != CryptoConstants.CompactPubkeyLen || compactBytes.All(b => b.Equals(0)))
            throw new ArgumentException("PublicKey cannot be empty.", nameof(compactBytes));
        
        CompactBytes = compactBytes;
    }
    
    public static implicit operator CompactPubKey(byte[] bytes) => new(bytes);
    public static implicit operator byte[](CompactPubKey hash) => hash.CompactBytes;
    
    public static implicit operator ReadOnlySpan<byte>(CompactPubKey compactPubKey) => compactPubKey.CompactBytes;
    public static implicit operator ReadOnlyMemory<byte>(CompactPubKey compactPubKey) => compactPubKey.CompactBytes;
    
    public override string ToString() => Convert.ToHexString(CompactBytes);
}