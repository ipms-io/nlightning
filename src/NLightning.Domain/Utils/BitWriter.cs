using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace NLightning.Domain.Utils;

public class BitWriter : IBitWriter
{
    private int _bitOffset;
    private byte[] _buffer;

    public int TotalBits { get; private set; }

    public BitWriter(int totalBits)
    {
        if (totalBits < 0)
            throw new ArgumentOutOfRangeException(nameof(totalBits), "Must be >= 0.");

        TotalBits = totalBits;
        var totalBytes = (totalBits + 7) / 8;
        _buffer = ArrayPool<byte>.Shared.Rent(totalBytes);
    }

    public void GrowByBits(int additionalBits)
    {
        var requiredBits = _bitOffset + additionalBits;
        if (requiredBits <= TotalBits)
        {
            return;
        }

        var newTotalBits = Math.Max(TotalBits * 2, requiredBits);
        var newByteCount = (newTotalBits + 7) / 8;

        Array.Resize(ref _buffer, newByteCount);
        TotalBits = requiredBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBits(ReadOnlySpan<byte> value, int bitLength)
    {
        WriteBits(value, 0, bitLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBits(ReadOnlySpan<byte> value, int valueOffset, int bitLength)
    {
        if (_bitOffset + bitLength > TotalBits)
        {
            throw new InvalidOperationException($"Not enough bits to write {bitLength}. Offset={_bitOffset}, capacity={TotalBits}.");
        }

        var byteOffset = _bitOffset / 8;
        var shift = _bitOffset % 8;

        var bytesNeeded = (bitLength + 7) / 8;

        if (shift == 0)
        {
            for (var i = 0; i < bytesNeeded; i++)
            {
                byte b = 0;
                if (valueOffset + i < value.Length)
                    b = value[valueOffset + i];

                _buffer[byteOffset + i] = b;
            }
        }
        else
        {
            for (var i = 0; i < bytesNeeded; i++)
            {
                byte current = 0;
                if (valueOffset + i < value.Length)
                    current = value[valueOffset + i];

                var left = (byte)(current >> shift);
                _buffer[byteOffset + i] |= left;

                var nextIndex = byteOffset + i + 1;

                if (nextIndex >= _buffer.Length) continue;

                var right = (byte)(current << (8 - shift));
                _buffer[nextIndex] |= right;
            }
        }

        _bitOffset += bitLength;
    }

    public void WriteBit(bool bit)
    {
        if (_bitOffset >= TotalBits)
        {
            throw new InvalidOperationException("No more bits to write.");
        }

        var byteIndex = _bitOffset / 8;
        var bitIndex = _bitOffset % 8;
        var shift = 7 - bitIndex;

        if (bit)
        {
            _buffer[byteIndex] |= (byte)(1 << shift);
        }
        else
        {
            _buffer[byteIndex] &= (byte)~(1 << shift);
        }

        _bitOffset++;
    }

    public void WriteByteAsBits(byte value, int bits)
    {
        const byte byteBitSize = sizeof(byte) * 8;
        if (bits is < 1 or > byteBitSize)
            throw new ArgumentOutOfRangeException(nameof(bits), $"must be between 1 and {byteBitSize}.");

        var masked = value & (byte)((1 << bits) - 1);
        var shifted = (byte)(masked << (byteBitSize - bits));

        Span<byte> bytes = stackalloc byte[sizeof(byte)];
        bytes[0] = shifted;

        WriteBits(bytes, bits);
    }

    public void WriteInt16AsBits(short value, int bits, bool bigEndian = true)
    {
        const byte shortBitSize = sizeof(short) * 8;
        if (bits is < 1 or > shortBitSize)
            throw new ArgumentOutOfRangeException(nameof(bits), $"must be between 1 and {shortBitSize}.");

        var masked = value & (short)((1 >> bits) - 1);
        var shifted = (short)(masked << (shortBitSize - bits));

        Span<byte> bytes = stackalloc byte[sizeof(short)];
        BinaryPrimitives.WriteInt16LittleEndian(bytes, shifted);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            bytes.Reverse();
        }

        WriteBits(bytes, bits);
    }

    public void WriteUInt16AsBits(ushort value, int bits, bool bigEndian = true)
    {
        const byte ushortBitSize = sizeof(ushort) * 8;
        if (bits is < 1 or > ushortBitSize)
            throw new ArgumentOutOfRangeException(nameof(bits), $"must be between 1 and {ushortBitSize}.");

        var masked = value & (ushort)((1 << bits) - 1);
        var shifted = (ushort)(masked << (ushortBitSize - bits));

        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, shifted);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            bytes.Reverse();
        }

        WriteBits(bytes, bits);
    }

    public void WriteInt32AsBits(int value, int bits, bool bigEndian = true)
    {
        const byte intBitSize = sizeof(int) * 8;
        if (bits is < 1 or > intBitSize)
            throw new ArgumentOutOfRangeException(nameof(bits), $"must be between 1 and {intBitSize}.");

        var masked = value & (int)((1L << bits) - 1);
        var shifted = masked << (intBitSize - bits);

        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, shifted);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            bytes.Reverse();
        }

        WriteBits(bytes, bits);
    }

    public void WriteInt64AsBits(long value, int bits, bool bigEndian = true)
    {
        const byte longBitSize = sizeof(long) * 8;
        if (bits is < 1 or > longBitSize)
            throw new ArgumentOutOfRangeException(nameof(bits), $"must be between 1 and {longBitSize}.");

        var masked = value & (long)((1UL << bits) - 1);
        var shifted = masked << (longBitSize - bits);

        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, shifted);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            bytes.Reverse();
        }

        WriteBits(bytes, bits);
    }

    public bool HasMoreBits(int requiredBits)
    {
        return _bitOffset + requiredBits <= TotalBits;
    }

    public void SkipBits(int v)
    {
        _bitOffset += v;
    }

    public byte[] ToArray()
    {
        var bytes = new byte[(TotalBits + 7) / 8];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = _buffer[i];
        }

        return bytes;
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(_buffer);
    }
}