namespace NLightning.Common.Types;

public readonly struct ChainHash
{
    public const int LENGTH = 32;

    private readonly byte[] _value;

    public ChainHash(byte[] value)
    {
        if (value.Length != 32)
        {
            throw new ArgumentException("ChainHash must be 32 bytes", nameof(value));
        }

        _value = value;
    }

    public readonly bool Equals(ChainHash other)
    {
        return _value.SequenceEqual(other._value);
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is ChainHash other)
        {
            return Equals(other);
        }

        return false;
    }

    public override readonly int GetHashCode()
    {
        return BitConverter.ToInt32(_value, 0);
    }

    public static implicit operator byte[](ChainHash c) => c._value;
    public static implicit operator ReadOnlyMemory<byte>(ChainHash c) => c._value;
    public static implicit operator ChainHash(byte[] value) => new(value);

    public static bool operator ==(ChainHash left, ChainHash right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ChainHash left, ChainHash right)
    {
        return !(left == right);
    }
}