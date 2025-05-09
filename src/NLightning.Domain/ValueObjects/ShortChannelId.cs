namespace NLightning.Domain.ValueObjects;

/// <summary>
/// Represents a short channel id.
/// </summary>
/// <remarks>
/// The short channel id is a unique description of the funding transaction.
/// </remarks>
public readonly struct ShortChannelId
{
    private readonly byte[] _value;

    public const int LENGTH = 8;

    public readonly uint BLOCK_HEIGHT;
    public readonly uint TRANSACTION_INDEX;
    public readonly ushort OUTPUT_INDEX;

    public ShortChannelId(uint blockHeight, uint transactionIndex, ushort outputIndex)
    {
        BLOCK_HEIGHT = blockHeight;
        TRANSACTION_INDEX = transactionIndex;
        OUTPUT_INDEX = outputIndex;

        _value = [
            (byte)(BLOCK_HEIGHT >> 16),
            (byte)(BLOCK_HEIGHT >> 8),
            (byte)BLOCK_HEIGHT,
            (byte)(TRANSACTION_INDEX >> 16),
            (byte)(TRANSACTION_INDEX >> 8),
            (byte)TRANSACTION_INDEX,
            (byte)(OUTPUT_INDEX >> 8),
            (byte)OUTPUT_INDEX
        ];
    }

    public ShortChannelId(byte[] value)
    {
        if (value.Length != LENGTH)
        {
            throw new ArgumentException($"ShortChannelId must be {LENGTH} bytes", nameof(value));
        }

        _value = value;

        BLOCK_HEIGHT = (uint)((value[0] << 16) | (value[1] << 8) | value[2]);
        TRANSACTION_INDEX = (uint)((value[3] << 16) | (value[4] << 8) | value[5]);
        OUTPUT_INDEX = (ushort)((value[6] << 8) | value[7]);
    }

    public ShortChannelId(ulong channelId) : this(
        (uint)((channelId >> 40) & 0xFFFFFF), // BLOCK_HEIGHT
        (uint)((channelId >> 16) & 0xFFFF),   // TRANSACTION_INDEX
        (ushort)(channelId & 0xFF)            // OUTPUT_INDEX
    )
    { }

    public ValueTask SerializeAsync(Stream stream)
    {
        return stream.WriteAsync(_value);
    }

    public static async Task<ShortChannelId> DeserializeAsync(Stream stream)
    {
        var buffer = new byte[LENGTH];
        await stream.ReadExactlyAsync(buffer);
        return new ShortChannelId(buffer);
    }

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
        return $"{BLOCK_HEIGHT}x{TRANSACTION_INDEX}x{OUTPUT_INDEX}";
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
        return BLOCK_HEIGHT == other.BLOCK_HEIGHT &&
               TRANSACTION_INDEX == other.TRANSACTION_INDEX &&
               OUTPUT_INDEX == other.OUTPUT_INDEX;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BLOCK_HEIGHT, TRANSACTION_INDEX, OUTPUT_INDEX);
    }
    #endregion

    #region Operators
    public static implicit operator byte[](ShortChannelId s) => s._value;
    public static implicit operator ShortChannelId(byte[] value) => new(value);
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