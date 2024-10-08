namespace NLightning.Common.Types;

using BitUtils;

/// <summary>
/// Represents a variable length integer.
/// </summary>
/// <param name="value">The value of the big size.</param>
/// <remarks>
/// Initializes a new instance of the <see cref="BigSize"/> struct.
/// </remarks>
public readonly struct BigSize(ulong value) : IComparable
{
    /// <summary>
    /// The uint representation of the big size.
    /// </summary>
    public ulong Value { get; } = value;

    #region Serialization
    /// <summary>
    /// Serializes a big size to a BinaryWriter.
    /// </summary>
    /// <param name="stream">The stream to serialize to.</param>
    public async Task SerializeAsync(Stream stream)
    {
        if (Value < 0xfd)
        {
            await stream.WriteAsync(new[] { (byte)Value });
        }
        else if (Value < 0x10000)
        {
            await stream.WriteAsync(new byte[] { 0xfd });
            await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)Value));
        }
        else if (Value < 0x100000000)
        {
            await stream.WriteAsync(new byte[] { 0xfe });
            await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((uint)Value));
        }
        else
        {
            await stream.WriteAsync(new byte[] { 0xff });
            await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Value));
        }
    }

    /// <summary>
    /// Deserializes a big size from a BinaryReader.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized big size.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is empty or insufficient data is available.</exception>
    public static async Task<BigSize> DeserializeAsync(Stream stream)
    {
        if (stream.Position == stream.Length)
        {
            throw new ArgumentException("BigSize cannot be read from an empty stream.");
        }

        var prefix = new byte[1];
        await stream.ReadExactlyAsync(prefix);
        ulong value;

        if (prefix[0] < 0xfd)
        {
            value = prefix[0];
        }
        else if (prefix[0] == 0xfd)
        {
            // Check if there are enough bytes to read
            if (stream.Position + 2 > stream.Length)
            {
                throw new ArgumentException("BigSize cannot be read from a stream with insufficient data.");
            }

            var bytes = new byte[2];
            await stream.ReadExactlyAsync(bytes);
            value = EndianBitConverter.ToUInt16BigEndian(bytes);
        }
        else if (prefix[0] == 0xfe)
        {
            if (stream.Position + 4 > stream.Length)
            {
                throw new ArgumentException("BigSize cannot be read from a stream with insufficient data.");
            }

            var bytes = new byte[4];
            await stream.ReadExactlyAsync(bytes);
            value = EndianBitConverter.ToUInt32BigEndian(bytes);
        }
        else
        {
            if (stream.Position + 8 > stream.Length)
            {
                throw new ArgumentException("BigSize cannot be read from a stream with insufficient data.");
            }

            var bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            value = EndianBitConverter.ToUInt64BigEndian(bytes);
        }

        return new BigSize(value);
    }
    #endregion

    #region Implicit Conversions
    public static implicit operator ulong(BigSize bigSize) => bigSize.Value;
    public static implicit operator BigSize(ulong value) => new(value);

    public static implicit operator long(BigSize bigSize)
    {
        if (bigSize.Value > long.MaxValue)
        {
            throw new OverflowException($"Cannot convert {bigSize.Value} to long because it's too large.");
        }

        return (long)bigSize.Value;
    }
    public static implicit operator BigSize(long value)
    {
        if (value < 0)
        {
            throw new OverflowException($"Cannot convert {value} to BigSize because it's negative.");
        }

        return new BigSize((ulong)value);
    }

    public static implicit operator uint(BigSize bigSize)
    {
        if (bigSize.Value > uint.MaxValue)
        {
            throw new OverflowException($"Cannot convert {bigSize.Value} to uint because it's too large.");
        }
        return (uint)bigSize.Value;
    }
    public static implicit operator BigSize(uint value) => new(value);

    public static implicit operator int(BigSize bigSize)
    {
        if (bigSize.Value > int.MaxValue)
        {
            throw new OverflowException($"Cannot convert {bigSize.Value} to int because it's too large.");
        }
        return (int)bigSize.Value;
    }
    public static implicit operator BigSize(int value)
    {
        if (value < 0)
        {
            throw new OverflowException($"Cannot convert {value} to BigSize because it's negative.");
        }
        return new BigSize((ulong)value);
    }

    public static implicit operator ushort(BigSize bigSize)
    {
        if (bigSize.Value > ushort.MaxValue)
        {
            throw new OverflowException($"Cannot convert {bigSize.Value} to ushort because it's too large.");
        }
        return (ushort)bigSize.Value;
    }
    public static implicit operator BigSize(ushort value) => new(value);

    public static implicit operator short(BigSize bigSize)
    {
        if (bigSize.Value > (ulong)short.MaxValue)
        {
            throw new OverflowException($"Cannot convert {bigSize.Value} to short because it's too large.");
        }
        return (short)bigSize.Value;
    }
    public static implicit operator BigSize(short value)
    {
        if (value < 0)
        {
            throw new OverflowException($"Cannot convert {value} to BigSize because it's negative.");
        }
        return new BigSize((ulong)value);
    }

    public static implicit operator byte(BigSize bigSize)
    {
        if (bigSize.Value > byte.MaxValue)
        {
            throw new OverflowException($"Cannot convert {bigSize.Value} to byte because it's too large.");
        }
        return (byte)bigSize.Value;
    }
    public static implicit operator BigSize(byte value) => new(value);
    #endregion

    #region Equality
    public override bool Equals(object? obj)
    {
        return obj is BigSize bigSize && Value == bigSize.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public int CompareTo(object? obj)
    {
        if (obj is not BigSize bigSize)
        {
            throw new ArgumentException("Object is not a BigSize");
        }

        return Value.CompareTo(bigSize.Value);
    }

    public static bool operator ==(BigSize left, BigSize right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(BigSize left, BigSize right)
    {
        return !(left == right);
    }

    public static bool operator <(BigSize left, BigSize right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >(BigSize left, BigSize right)
    {
        return left.Value > right.Value;
    }

    public static bool operator <=(BigSize left, BigSize right)
    {
        return left.Value <= right.Value;
    }

    public static bool operator >=(BigSize left, BigSize right)
    {
        return left.Value >= right.Value;
    }
    #endregion
}