namespace NLightning.Domain.ValueObjects;

using Interfaces;

/// <summary>
/// Represents a short channel id.
/// </summary>
/// <remarks>
/// The short channel id is a unique description of the funding transaction.
/// </remarks>
public readonly struct ShortChannelId : IValueObject, IEquatable<ShortChannelId>
{
    private readonly byte[] _value;

    public const int LENGTH = 8;

    public readonly uint BlockHeight;
    public readonly uint TransactionIndex;
    public readonly ushort OutputIndex;

    public ShortChannelId(uint blockHeight, uint transactionIndex, ushort outputIndex)
    {
        BlockHeight = blockHeight;
        TransactionIndex = transactionIndex;
        OutputIndex = outputIndex;

        _value = [
            (byte)(BlockHeight >> 16),
            (byte)(BlockHeight >> 8),
            (byte)BlockHeight,
            (byte)(TransactionIndex >> 16),
            (byte)(TransactionIndex >> 8),
            (byte)TransactionIndex,
            (byte)(OutputIndex >> 8),
            (byte)OutputIndex
        ];
    }

    public ShortChannelId(byte[] value)
    {
        if (value.Length != LENGTH)
        {
            throw new ArgumentException($"ShortChannelId must be {LENGTH} bytes", nameof(value));
        }

        _value = value;

        BlockHeight = (uint)((value[0] << 16) | (value[1] << 8) | value[2]);
        TransactionIndex = (uint)((value[3] << 16) | (value[4] << 8) | value[5]);
        OutputIndex = (ushort)((value[6] << 8) | value[7]);
    }

    public ShortChannelId(ulong channelId) : this(
        (uint)((channelId >> 40) & 0xFFFFFF), // BLOCK_HEIGHT
        (uint)((channelId >> 16) & 0xFFFF),   // TRANSACTION_INDEX
        (ushort)(channelId & 0xFF)            // OUTPUT_INDEX
    ) { }

    public static ShortChannelId Parse(string shortChannelId)
    {
        var parts = shortChannelId.Split('x');
        if (parts.Length != 3)
        {
            throw new FormatException("Invalid short_channel_id format");
        }

        return new ShortChannelId(
            uint.Parse(parts[0]),
            uint.Parse(parts[1]),
            ushort.Parse(parts[2])
        );
    }

    #region Overrides
    public override string ToString()
    {
        return $"{BlockHeight}x{TransactionIndex}x{OutputIndex}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is ShortChannelId other)
        {
            return Equals(other);
        }

        return false;
    }

    public bool Equals(ShortChannelId other)
    {
        return BlockHeight == other.BlockHeight &&
               TransactionIndex == other.TransactionIndex &&
               OutputIndex == other.OutputIndex;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BlockHeight, TransactionIndex, OutputIndex);
    }
    #endregion

    #region Implicit Operators
    public static implicit operator byte[](ShortChannelId s) => s._value;
    public static implicit operator ShortChannelId(byte[] value) => new(value);
    public static implicit operator ReadOnlyMemory<byte>(ShortChannelId s) => s._value;
    public static implicit operator ReadOnlySpan<byte>(ShortChannelId s) => s._value;
    public static implicit operator ShortChannelId(Span<byte> value) => new(value.ToArray());
    public static implicit operator ShortChannelId(ulong value) => new(value);

    public static bool operator ==(ShortChannelId left, ShortChannelId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShortChannelId left, ShortChannelId right)
    {
        return !(left == right);
    }
    #endregion
}