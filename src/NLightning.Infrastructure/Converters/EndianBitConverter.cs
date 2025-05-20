namespace NLightning.Infrastructure.Converters;

public static class EndianBitConverter
{
    #region GetBytesBE
    /// <summary>
    /// Converts a ulong to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The ulong to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the ulong.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBigEndian(ulong value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (!trimToMinimumLenght)
        {
            return bytes;
        }

        var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
        return firstNonZeroIndex == -1 ? [0] : bytes[firstNonZeroIndex..];
    }

    /// <summary>
    /// Converts a long to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The long to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the long.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBigEndian(long value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (!trimToMinimumLenght)
        {
            return bytes;
        }

        var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
        return firstNonZeroIndex == -1 ? [0] : bytes[firstNonZeroIndex..];
    }

    /// <summary>
    /// Converts a uint to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The uint to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the uint.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBigEndian(uint value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (!trimToMinimumLenght)
        {
            return bytes;
        }

        var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
        return firstNonZeroIndex == -1 ? [0] : bytes[firstNonZeroIndex..];
    }

    /// <summary>
    /// Converts a int to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The int to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the int.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBigEndian(int value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (!trimToMinimumLenght)
        {
            return bytes;
        }

        var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
        return firstNonZeroIndex == -1 ? [0] : bytes[firstNonZeroIndex..];
    }

    /// <summary>
    /// Converts a ushort to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The ushort to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the ushort.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBigEndian(ushort value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (!trimToMinimumLenght)
        {
            return bytes;
        }

        var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
        return firstNonZeroIndex == -1 ? [0] : bytes[firstNonZeroIndex..];
    }

    /// <summary>
    /// Converts a short to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The short to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the short.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBigEndian(short value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (!trimToMinimumLenght)
        {
            return bytes;
        }

        var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
        return firstNonZeroIndex == -1 ? [0] : bytes[firstNonZeroIndex..];
    }
    #endregion

    #region GetBytesLE
    /// <summary>
    /// Converts a ulong to a byte array in little-endian order.
    /// </summary>
    /// <param name="value">The ulong to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the ulong.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesLittleEndian(ulong value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (!trimToMinimumLenght)
        {
            return bytes;
        }

        var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
        return firstNonZeroIndex == -1 ? [0] : bytes[firstNonZeroIndex..];
    }

    /// <summary>
    /// Converts a uint to a byte array in little-endian order.
    /// </summary>
    /// <param name="value">The uint to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the uint.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesLittleEndian(uint value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (!trimToMinimumLenght)
        {
            return bytes;
        }

        var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
        return firstNonZeroIndex == -1 ? [0] : bytes[firstNonZeroIndex..];
    }

    /// <summary>
    /// Converts a ushort to a byte array in little-endian order.
    /// </summary>
    /// <param name="value">The ushort to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the ushort.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesLittleEndian(ushort value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (!trimToMinimumLenght)
        {
            return bytes;
        }

        var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
        return firstNonZeroIndex == -1 ? [0] : bytes[firstNonZeroIndex..];
    }
    #endregion

    #region Back From LE Bytes
    /// <summary>
    /// Converts a byte array to an ulong in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 8.</param>
    /// <returns>The ulong representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 8.</remarks>
    public static ulong ToUInt64LittleEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 8
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 8)
        {
            paddedBytes = new byte[8];
            bytes.CopyTo(paddedBytes.AsSpan()[(8 - bytes.Length)..]);
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToUInt64(paddedBytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ulong in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 8.</param>
    /// <returns>The ulong representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 8.</remarks>
    public static long ToInt64LittleEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 8
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 8)
        {
            paddedBytes = new byte[8];
            bytes.CopyTo(paddedBytes.AsSpan()[(8 - bytes.Length)..]);
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToInt64(paddedBytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a uint in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 4.</param>
    /// <returns>The uint representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 4.</remarks>
    public static uint ToUInt32LittleEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 4
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 4)
        {
            paddedBytes = new byte[4];
            bytes.CopyTo(paddedBytes.AsSpan()[(4 - bytes.Length)..]);
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToUInt32(paddedBytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a uint in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 4.</param>
    /// <returns>The uint representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 4.</remarks>
    public static int ToInt32LittleEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 4
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 4)
        {
            paddedBytes = new byte[4];
            bytes.CopyTo(paddedBytes.AsSpan()[(4 - bytes.Length)..]);
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToInt32(paddedBytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ushort in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 2.</param>
    /// <returns>The ushort representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 2.</remarks>
    public static ushort ToUInt16LittleEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 2
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 2)
        {
            paddedBytes = new byte[2];
            bytes.CopyTo(paddedBytes.AsSpan()[(2 - bytes.Length)..]);
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToUInt16(paddedBytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ushort in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 2.</param>
    /// <returns>The ushort representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 2.</remarks>
    public static short ToInt16LittleEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 2
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 2)
        {
            paddedBytes = new byte[2];
            bytes.CopyTo(paddedBytes.AsSpan()[(2 - bytes.Length)..]);
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToInt16(paddedBytes, 0);
    }
    #endregion

    #region Back From BE Bytes
    /// <summary>
    /// Converts a byte array to a ulong in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 8.</param>
    /// <returns>The ulong representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 8.</remarks>
    public static ulong ToUInt64BigEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 8
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 8)
        {
            paddedBytes = new byte[8];
            bytes.CopyTo(paddedBytes.AsSpan()[(8 - bytes.Length)..]);
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToUInt64(paddedBytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ulong in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 8.</param>
    /// <returns>The ulong representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 8.</remarks>
    public static long ToInt64BigEndian(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 8
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 8)
        {
            paddedBytes = new byte[8];
            bytes.CopyTo(paddedBytes.AsSpan()[(8 - bytes.Length)..]);
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToInt64(paddedBytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a uint in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 4.</param>
    /// <returns>The uint representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 4.</remarks>
    public static uint ToUInt32BigEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 4
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 4)
        {
            paddedBytes = new byte[4];
            bytes.CopyTo(paddedBytes.AsSpan()[(4 - bytes.Length)..]);
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToUInt32(paddedBytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a uint in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 4.</param>
    /// <returns>The uint representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 4.</remarks>
    public static int ToInt32BigEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 4
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 4)
        {
            paddedBytes = new byte[4];
            bytes.CopyTo(paddedBytes.AsSpan()[(4 - bytes.Length)..]);
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToInt32(paddedBytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ushort in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 2.</param>
    /// <returns>The ushort representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 2.</remarks>
    public static ushort ToUInt16BigEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 2
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 2)
        {
            paddedBytes = new byte[2];
            bytes.CopyTo(paddedBytes.AsSpan()[(2 - bytes.Length)..]);
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToUInt16(paddedBytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ushort in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 2.</param>
    /// <returns>The ushort representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 2.</remarks>
    public static short ToInt16BigEndian(ReadOnlySpan<byte> bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 2
        var paddedBytes = bytes.ToArray();
        if (padWithZero && bytes.Length < 2)
        {
            paddedBytes = new byte[2];
            bytes.CopyTo(paddedBytes.AsSpan()[(2 - bytes.Length)..]);
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(paddedBytes);
        }
        return BitConverter.ToInt16(paddedBytes, 0);
    }
    #endregion
}