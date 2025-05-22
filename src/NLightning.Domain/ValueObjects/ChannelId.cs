namespace NLightning.Domain.ValueObjects;

using Interfaces;

/// <summary>
/// Represents a channel id.
/// </summary>
/// <remarks>
/// The channel id is a unique identifier for a channel.
/// </remarks>
public readonly record struct ChannelId : IValueObject
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
    public override string ToString()
    {
        return Convert.ToHexString(_value);
    }
    #endregion

    #region Implicit Conversions
    public static implicit operator byte[](ChannelId c) => c._value;
    public static implicit operator ReadOnlyMemory<byte>(ChannelId c) => c._value;
    public static implicit operator ChannelId(byte[] value) => new(value);
    public static implicit operator ChannelId(Span<byte> value) => new(value);
    #endregion
}