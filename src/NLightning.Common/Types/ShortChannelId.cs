namespace NLightning.Common.Types;

/// <summary>
/// Represents a short channel id.
/// </summary>
/// <remarks>
/// The short channel id is a unique description of the funding transaction.
/// </remarks>
public readonly struct ShortChannelId
{
    public const int LENGTH = 8;

    public readonly uint BLOCK_HEIGHT;
    public readonly uint TRANSACTION_INDEX;
    public readonly ushort OUTPUT_INDEX;

    public ShortChannelId(uint blockHeight, uint transactionIndex, ushort outputIndex)
    {
        BLOCK_HEIGHT = blockHeight;
        TRANSACTION_INDEX = transactionIndex;
        OUTPUT_INDEX = outputIndex;
    }

    public ShortChannelId(byte[] value)
    {
        if (value.Length != LENGTH)
        {
            throw new ArgumentException($"ShortChannelId must be {LENGTH} bytes", nameof(value));
        }

        BLOCK_HEIGHT = (uint)((value[0] << 16) | (value[1] << 8) | value[2]);
        TRANSACTION_INDEX = (uint)((value[3] << 16) | (value[4] << 8) | value[5]);
        OUTPUT_INDEX = (ushort)((value[6] << 8) | value[7]);
    }

    public ValueTask SerializeAsync(Stream stream)
    {
        return stream.WriteAsync(ToByteArray());
    }

    public static async Task<ShortChannelId> DeserializeAsync(Stream stream)
    {
        var buffer = new byte[LENGTH];
        _ = await stream.ReadAsync(buffer);
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

    public readonly byte[] ToByteArray()
    {
        return
        [
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

    #region Overrides
    public override readonly string ToString()
    {
        return $"{BLOCK_HEIGHT}x{TRANSACTION_INDEX}x{OUTPUT_INDEX}";
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is ShortChannelId other)
        {
            return Equals(other);
        }

        return false;
    }

    public readonly bool Equals(ShortChannelId other)
    {
        return BLOCK_HEIGHT == other.BLOCK_HEIGHT &&
               TRANSACTION_INDEX == other.TRANSACTION_INDEX &&
               OUTPUT_INDEX == other.OUTPUT_INDEX;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(BLOCK_HEIGHT, TRANSACTION_INDEX, OUTPUT_INDEX);
    }
    #endregion

    #region Operators
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