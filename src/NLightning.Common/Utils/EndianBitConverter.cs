namespace NLightning.Common.Utils;

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
    public static byte[] GetBytesBE(ulong value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (trimToMinimumLenght)
        {
            var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
            if (firstNonZeroIndex == -1)
            {
                return [0];
            }
            return bytes[firstNonZeroIndex..];
        }

        return bytes;
    }

    /// <summary>
    /// Converts a long to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The long to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the long.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBE(long value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (trimToMinimumLenght)
        {
            var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
            if (firstNonZeroIndex == -1)
            {
                return [0];
            }
            return bytes[firstNonZeroIndex..];
        }

        return bytes;
    }

    /// <summary>
    /// Converts a uint to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The uint to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the uint.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBE(uint value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (trimToMinimumLenght)
        {
            var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
            if (firstNonZeroIndex == -1)
            {
                return [0];
            }
            return bytes[firstNonZeroIndex..];
        }

        return bytes;
    }

    /// <summary>
    /// Converts a int to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The int to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the int.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBE(int value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (trimToMinimumLenght)
        {
            var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
            if (firstNonZeroIndex == -1)
            {
                return [0];
            }
            return bytes[firstNonZeroIndex..];
        }

        return bytes;
    }

    /// <summary>
    /// Converts a ushort to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The ushort to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the ushort.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBE(ushort value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (trimToMinimumLenght)
        {
            var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
            if (firstNonZeroIndex == -1)
            {
                return [0];
            }
            return bytes[firstNonZeroIndex..];
        }

        return bytes;
    }

    /// <summary>
    /// Converts a short to a byte array in big-endian order.
    /// </summary>
    /// <param name="value">The short to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the short.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesBE(short value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (trimToMinimumLenght)
        {
            var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
            if (firstNonZeroIndex == -1)
            {
                return [0];
            }
            return bytes[firstNonZeroIndex..];
        }

        return bytes;
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
    public static byte[] GetBytesLE(ulong value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (trimToMinimumLenght)
        {
            var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
            if (firstNonZeroIndex == -1)
            {
                return [0];
            }
            return bytes[firstNonZeroIndex..];
        }

        return bytes;
    }

    /// <summary>
    /// Converts a uint to a byte array in little-endian order.
    /// </summary>
    /// <param name="value">The uint to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the uint.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesLE(uint value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (trimToMinimumLenght)
        {
            var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
            if (firstNonZeroIndex == -1)
            {
                return [0];
            }
            return bytes[firstNonZeroIndex..];
        }

        return bytes;
    }

    /// <summary>
    /// Converts a ushort to a byte array in little-endian order.
    /// </summary>
    /// <param name="value">The ushort to convert.</param>
    /// <param name="trimToMinimumLenght">If true, the byte array will be trimmed to the minimum length.</param>
    /// <returns>The byte array representation of the ushort.</returns>
    /// <remarks>Trimming to minimum length is useful when the byte array is used in a context where the length is known.</remarks>
    public static byte[] GetBytesLE(ushort value, bool trimToMinimumLenght = false)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        if (trimToMinimumLenght)
        {
            var firstNonZeroIndex = Array.FindIndex(bytes, b => b != 0);
            if (firstNonZeroIndex == -1)
            {
                return [0];
            }
            return bytes[firstNonZeroIndex..];
        }

        return bytes;
    }
    #endregion

    #region Back From LEBytes
    /// <summary>
    /// Converts a byte array to a ulong in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 8.</param>
    /// <returns>The ulong representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 8.</remarks>
    public static ulong ToUInt64LE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 8
        if (padWithZero && bytes.Length < 8)
        {
            var paddedBytes = new byte[8];
            bytes.CopyTo(paddedBytes, 8 - bytes.Length);
            bytes = paddedBytes;
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToUInt64(bytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ulong in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 8.</param>
    /// <returns>The ulong representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 8.</remarks>
    public static long ToInt64LE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 8
        if (padWithZero && bytes.Length < 8)
        {
            var paddedBytes = new byte[8];
            bytes.CopyTo(paddedBytes, 8 - bytes.Length);
            bytes = paddedBytes;
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt64(bytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a uint in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 4.</param>
    /// <returns>The uint representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 4.</remarks>
    public static uint ToUInt32LE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 4
        if (padWithZero && bytes.Length < 4)
        {
            var paddedBytes = new byte[4];
            bytes.CopyTo(paddedBytes, 4 - bytes.Length);
            bytes = paddedBytes;
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToUInt32(bytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a uint in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 4.</param>
    /// <returns>The uint representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 4.</remarks>
    public static int ToInt32LE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 4
        if (padWithZero && bytes.Length < 4)
        {
            var paddedBytes = new byte[4];
            bytes.CopyTo(paddedBytes, 4 - bytes.Length);
            bytes = paddedBytes;
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt32(bytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ushort in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 2.</param>
    /// <returns>The ushort representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 2.</remarks>
    public static ushort ToUInt16LE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 2
        if (padWithZero && bytes.Length < 2)
        {
            var paddedBytes = new byte[2];
            bytes.CopyTo(paddedBytes, 2 - bytes.Length);
            bytes = paddedBytes;
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToUInt16(bytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ushort in little-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 2.</param>
    /// <returns>The ushort representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 2.</remarks>
    public static short ToInt16LE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 2
        if (padWithZero && bytes.Length < 2)
        {
            var paddedBytes = new byte[2];
            bytes.CopyTo(paddedBytes, 2 - bytes.Length);
            bytes = paddedBytes;
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt16(bytes, 0);
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
    public static ulong ToUInt64BE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 8
        if (padWithZero && bytes.Length < 8)
        {
            var paddedBytes = new byte[8];
            bytes.CopyTo(paddedBytes, 8 - bytes.Length);
            bytes = paddedBytes;
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToUInt64(bytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ulong in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 8.</param>
    /// <returns>The ulong representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 8.</remarks>
    public static long ToInt64BE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 8
        if (padWithZero && bytes.Length < 8)
        {
            var paddedBytes = new byte[8];
            bytes.CopyTo(paddedBytes, 8 - bytes.Length);
            bytes = paddedBytes;
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt64(bytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a uint in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 4.</param>
    /// <returns>The uint representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 4.</remarks>
    public static uint ToUInt32BE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 4
        if (padWithZero && bytes.Length < 4)
        {
            var paddedBytes = new byte[4];
            bytes.CopyTo(paddedBytes, 4 - bytes.Length);
            bytes = paddedBytes;
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToUInt32(bytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a uint in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 4.</param>
    /// <returns>The uint representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 4.</remarks>
    public static int ToInt32BE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 4
        if (padWithZero && bytes.Length < 4)
        {
            var paddedBytes = new byte[4];
            bytes.CopyTo(paddedBytes, 4 - bytes.Length);
            bytes = paddedBytes;
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt32(bytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ushort in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 2.</param>
    /// <returns>The ushort representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 2.</remarks>
    public static ushort ToUInt16BE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 2
        if (padWithZero && bytes.Length < 2)
        {
            var paddedBytes = new byte[2];
            bytes.CopyTo(paddedBytes, 2 - bytes.Length);
            bytes = paddedBytes;
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToUInt16(bytes, 0);
    }

    /// <summary>
    /// Converts a byte array to a ushort in big-endian order.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="padWithZero">If true, the byte array will be padded with zero if the length is less than 2.</param>
    /// <returns>The ushort representation of the byte array.</returns>
    /// <remarks>Padding with zero is useful when the byte array is used in a context where the length is known to be less than 2.</remarks>
    public static short ToInt16BE(byte[] bytes, bool padWithZero = false)
    {
        // pad with zero if the length is less than 2
        if (padWithZero && bytes.Length < 2)
        {
            var paddedBytes = new byte[2];
            bytes.CopyTo(paddedBytes, 2 - bytes.Length);
            bytes = paddedBytes;
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt16(bytes, 0);
    }
    #endregion
}