namespace NLightning.Domain.ValueObjects;

using Interfaces;

/// <summary>
/// Represents a channel id.
/// </summary>
/// <remarks>
/// The channel id is a unique identifier for a channel.
/// </remarks>
public readonly struct ChannelId : IValueObject, IEquatable<ChannelId>
{
    public const int LENGTH = 32;

    private readonly byte[] _value;

    public static ChannelId Zero => new(new byte[LENGTH]);

    public ChannelId(ReadOnlySpan<byte> value)
    {
        if (value.Length != LENGTH)
        {
            throw new ArgumentException($"ChannelId must be {LENGTH} bytes", nameof(value));
        }

        _value = value.ToArray();
    }

    #region Overrides
    public override bool Equals(object? obj)
    {
        if (obj is ChannelId other)
        {
            return Equals(other);
        }

        return false;
    }

    public bool Equals(ChannelId other) => _value.SequenceEqual(other._value);

    public override int GetHashCode()
    {
        return BitConverter.ToInt32(_value, 0);
    }
    #endregion

    #region Operators
    public static implicit operator byte[](ChannelId c) => c._value;
    public static implicit operator ReadOnlyMemory<byte>(ChannelId c) => c._value;
    public static implicit operator ChannelId(byte[] value) => new(value);
    public static implicit operator ChannelId(Span<byte> value) => new(value);

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