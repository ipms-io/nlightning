namespace NLightning.Common;

public readonly struct ChainHash
{
    private readonly byte[] Value;

    public ChainHash(byte[] value)
    {
        if (value.Length != 32)
        {
            throw new ArgumentException("ChainHash must be 32 bytes", nameof(value));
        }

        Value = value;
    }

    public readonly bool Equals(ChainHash other)
    {
        return Value.SequenceEqual(other.Value);
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
        return BitConverter.ToInt32(Value, 0);
    }

    public static implicit operator byte[](ChainHash c) => c.Value;
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