namespace NLightning.Domain.ValueObjects;

using Interfaces;

/// <summary>
/// Represents a variable length integer.
/// </summary>
/// <param name="value">The value of the big size.</param>
/// <remarks>
/// Initializes a new instance of the <see cref="BigSize"/> struct.
/// </remarks>
public readonly struct BigSize(ulong value) : IValueObject, IComparable, IEquatable<BigSize>
{
    /// <summary>
    /// The uint representation of the big size.
    /// </summary>
    public ulong Value { get; } = value;

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
    public bool Equals(BigSize other)
    {
        return Value == other.Value;
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