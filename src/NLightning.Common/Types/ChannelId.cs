namespace NLightning.Common.Types;

/// <summary>
/// Represents a channel id.
/// </summary>
/// <remarks>
/// The channel id is a unique identifier for a channel.
/// </remarks>
public readonly struct ChannelId
{
    public const int LENGTH = 32;

    private readonly byte[] _value;

    public static ChannelId Zero => new(new byte[LENGTH]);

    public ChannelId(byte[] value)
    {
        if (value.Length != LENGTH)
        {
            throw new ArgumentException($"ChannelId must be {LENGTH} bytes", nameof(value));
        }

        _value = value;
    }

    public ValueTask SerializeAsync(Stream stream)
    {
        return stream.WriteAsync(_value);
    }

    public static async Task<ChannelId> DeserializeAsync(Stream stream)
    {
        var buffer = new byte[LENGTH];
        await stream.ReadAsync(buffer);
        return new ChannelId(buffer);
    }

    #region Overrides
    public override readonly bool Equals(object? obj)
    {
        if (obj is ChannelId other)
        {
            return Equals(other);
        }

        return false;
    }

    public readonly bool Equals(ChannelId other) => _value.SequenceEqual(other._value);

    public override readonly int GetHashCode()
    {
        return BitConverter.ToInt32(_value, 0);
    }
    #endregion

    #region Operators
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
    #endregion
}