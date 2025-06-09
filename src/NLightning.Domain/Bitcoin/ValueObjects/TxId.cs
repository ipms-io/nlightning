namespace NLightning.Domain.Bitcoin.ValueObjects;

using Crypto.Constants;
using Utils.Extensions;

public struct TxId : IEquatable<TxId>
{
    public byte[] Hash { get; }

    public bool IsZero => Hash.SequenceEqual(Zero.Hash);
    public bool IsOne => Hash.SequenceEqual(One.Hash);

    public TxId(byte[] hash)
    {
        if (hash.Length < CryptoConstants.Sha256HashLen)
            throw new ArgumentException("TxId cannot be empty.", nameof(hash));

        Hash = hash;
    }

    public static TxId Zero => new byte[CryptoConstants.Sha256HashLen];

    public static TxId One => new byte[]
    {
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
    };

    public static implicit operator TxId(byte[] bytes) => new(bytes);
    public static implicit operator byte[](TxId txId) => txId.Hash;
    public static implicit operator ReadOnlyMemory<byte>(TxId compactPubKey) => compactPubKey.Hash;

    public static bool operator !=(TxId left, TxId right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(TxId left, TxId right)
    {
        return left.Equals(right);
    }

    public bool Equals(TxId other)
    {
        return Hash.SequenceEqual(other.Hash);
    }

    public override bool Equals(object? obj)
    {
        return obj is TxId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Hash.GetByteArrayHashCode();
    }
}