namespace NLightning.Common;

public readonly struct ChannelId
{
    public const int LENGTH = 32;

    private readonly byte[] _value;

    public static ChannelId Zero => new([LENGTH]);

    public ChannelId(byte[] value)
    {
        if (value.Length != LENGTH)
        {
            throw new ArgumentException($"ChannelId must be {LENGTH} bytes", nameof(value));
        }

        _value = value;
    }
    public ChannelId(BinaryReader reader)
    {
        _value = reader.ReadBytes(LENGTH);
    }

    public ValueTask SerializeAsync(Stream stream)
    {
        return stream.WriteAsync(_value);
    }

    public readonly bool Equals(ChannelId other)
    {
        return _value.SequenceEqual(other._value);
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is ChannelId other)
        {
            return Equals(other);
        }

        return false;
    }

    public override readonly int GetHashCode()
    {
        return BitConverter.ToInt32(_value, 0);
    }

    public static implicit operator byte[](ChannelId c) => c._value;
    public static implicit operator ChannelId(byte[] value) => new(value);

    public static bool operator ==(ChannelId left, ChannelId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ChannelId left, ChannelId right)
    {
        return !(left == right);
    }
}