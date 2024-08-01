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

    public readonly uint BlockHeight;
    public readonly uint TransactionIndex;
    public readonly ushort OutputIndex;

    public ShortChannelId(uint blockHeight, uint transactionIndex, ushort outputIndex)
    {
        BlockHeight = blockHeight;
        TransactionIndex = transactionIndex;
        OutputIndex = outputIndex;
    }

    public ShortChannelId(byte[] value)
    {
        if (value.Length != LENGTH)
        {
            throw new ArgumentException($"ShortChannelId must be {LENGTH} bytes", nameof(value));
        }

        BlockHeight = (uint)((value[0] << 16) | (value[1] << 8) | value[2]);
        TransactionIndex = (uint)((value[3] << 16) | (value[4] << 8) | value[5]);
        OutputIndex = (ushort)((value[6] << 8) | value[7]);
    }

    public ValueTask SerializeAsync(Stream stream)
    {
        return stream.WriteAsync(ToByteArray());
    }

    public static async Task<ShortChannelId> DeserializeAsync(Stream stream)
    {
        var buffer = new byte[LENGTH];
        await stream.ReadAsync(buffer);
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

    #region Overrides
    public override readonly string ToString()
    {
        return $"{BlockHeight}x{TransactionIndex}x{OutputIndex}";
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
        return BlockHeight == other.BlockHeight &&
               TransactionIndex == other.TransactionIndex &&
               OutputIndex == other.OutputIndex;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(BlockHeight, TransactionIndex, OutputIndex);
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