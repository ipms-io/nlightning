namespace NLightning.Domain.Bitcoin.ValueObjects;

using Crypto.Constants;
using Utils.Extensions;

public readonly struct TxId : IEquatable<TxId>
{
    private readonly byte[] _value;

    public bool IsZero => _value.SequenceEqual(Zero._value);
    public bool IsOne => _value.SequenceEqual(One._value);

    public TxId(byte[] hash)
    {
        if (hash.Length < CryptoConstants.Sha256HashLen)
            throw new ArgumentException("TxId cannot be empty.", nameof(hash));

        _value = hash;
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
    public static implicit operator byte[](TxId txId) => txId._value;
    public static implicit operator ReadOnlyMemory<byte>(TxId compactPubKey) => compactPubKey._value;
    public static implicit operator ReadOnlySpan<byte>(TxId compactPubKey) => compactPubKey._value;

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
        return _value.SequenceEqual(other._value);
    }

    public override bool Equals(object? obj)
    {
        return obj is TxId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetByteArrayHashCode();
    }

    public override string ToString()
    {
        return Convert.ToHexString(_value).ToLowerInvariant();
    }
}