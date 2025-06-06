namespace NLightning.Domain.Bitcoin.ValueObjects;

using Domain.Interfaces;
using Domain.Utils.Extensions;

public readonly struct BitcoinScript : IValueObject, IEquatable<BitcoinScript>
{
    private readonly byte[] _value;

    public static BitcoinScript Empty => new([]);

    public int Length => _value.Length;

    public BitcoinScript(byte[] value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value), "BitcoinScript cannot be null.");
    }

    public static implicit operator BitcoinScript(byte[] bytes) => new(bytes);
    public static implicit operator byte[](BitcoinScript script) => script._value;
    public static implicit operator ReadOnlyMemory<byte>(BitcoinScript compactPubKey) => compactPubKey._value;

    public override string ToString()
    {
        return Convert.ToHexString(_value).ToLowerInvariant();
    }

    public bool Equals(BitcoinScript other)
    {
        return _value.SequenceEqual(other._value);
    }

    public override bool Equals(object? obj)
    {
        return obj is BitcoinScript other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetByteArrayHashCode();
    }

    public static bool operator !=(BitcoinScript left, BitcoinScript right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(BitcoinScript left, BitcoinScript right)
    {
        return left.Equals(right);
    }
}