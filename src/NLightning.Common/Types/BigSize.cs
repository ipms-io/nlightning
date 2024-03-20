namespace NLightning.Common.Types;

using Utils;

/// <summary>
/// Represents a variable length integer.
/// </summary>
/// <param name="value">The value of the big size.</param>
/// <remarks>
/// Initializes a new instance of the <see cref="BigSize"/> struct.
/// </remarks>
/// <param name="value">The value of the big size.</param>
public readonly struct BigSize(ulong value)
{
    /// <summary>
    /// The uint representation of the big size.
    /// </summary>
    public ulong Value { get; } = value;

    #region Serialization
    /// <summary>
    /// Serializes a big size to a BinaryWriter.
    /// </summary>
    /// <param name="writer">The writer to serialize to.</param>
    public void Serialize(BinaryWriter writer)
    {
        if (Value < 0xfd)
        {
            writer.Write((byte)Value);
        }
        else if (Value < 0x10000)
        {
            writer.Write((byte)0xfd);
            writer.Write(EndianBitConverter.GetBytesBE((ushort)Value));
        }
        else if (Value < 0x100000000)
        {
            writer.Write((byte)0xfe);
            writer.Write(EndianBitConverter.GetBytesBE((uint)Value));
        }
        else
        {
            writer.Write((byte)0xff);
            writer.Write(EndianBitConverter.GetBytesBE(Value));
        }
    }

    /// <summary>
    /// Deserializes a big size from a BinaryReader.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized big size.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is empty or insufficient data is available.</exception>
    public static BigSize Deserialize(BinaryReader reader)
    {
        if (reader.BaseStream.Position == reader.BaseStream.Length)
        {
            throw new ArgumentException("BigSize cannot be read from an empty stream.");
        }

        var prefix = reader.ReadByte();
        ulong value;

        if (prefix < 0xfd)
        {
            value = prefix;
        }
        else if (prefix == 0xfd)
        {
            // Check if there are enough bytes to read
            if (reader.BaseStream.Position + 2 > reader.BaseStream.Length)
            {
                throw new ArgumentException("BigSize cannot be read from a stream with insufficient data.");
            }

            value = EndianBitConverter.ToUInt16BE(reader.ReadBytes(2));
        }
        else if (prefix == 0xfe)
        {
            if (reader.BaseStream.Position + 4 > reader.BaseStream.Length)
            {
                throw new ArgumentException("BigSize cannot be read from a stream with insufficient data.");
            }

            value = EndianBitConverter.ToUInt32BE(reader.ReadBytes(4));
        }
        else
        {
            if (reader.BaseStream.Position + 8 > reader.BaseStream.Length)
            {
                throw new ArgumentException("BigSize cannot be read from a stream with insufficient data.");
            }

            value = EndianBitConverter.ToUInt64BE(reader.ReadBytes(8));
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