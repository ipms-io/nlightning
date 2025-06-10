using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace NLightning.Domain.Utils;

using Interfaces;

public class BitReader(byte[] buffer) : IBitReader
{
    private readonly byte[] _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
    private readonly int _totalBits = buffer.Length * 8;

    private int _bitOffset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadBits(Span<byte> value, int bitLength)
    {
        return ReadBits(value, 0, bitLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ReadBits(Span<byte> value, int valueOffset, int bitLength)
    {
        if (_bitOffset + bitLength > _totalBits)
        {
            throw new InvalidOperationException(
                $"Not enough bits left to read. Requested {bitLength}, but only {_totalBits - _bitOffset} remain.");
        }

        var byteOffset = _bitOffset / 8;
        var shift = (int)((uint)_bitOffset % 8);

        if (shift == 0)
        {
            // copy bytes to value 
            _buffer.AsSpan(byteOffset, bitLength / 8 + (bitLength % 8 == 0 ? 0 : 1)).CopyTo(value[valueOffset..]);

            // mask extra bits 
            if (bitLength % 8 != 0)
            {
                value[valueOffset + bitLength / 8] &= (byte)(0xFF << (8 - bitLength % 8));
            }
        }
        else
        {
            var bytesToRead = bitLength / 8 + (shift > 0 ? 0 : -1);
            var maxBytesToRead = value.Length - 1 - valueOffset;
            if (bytesToRead > maxBytesToRead)
            {
                bytesToRead = maxBytesToRead;
            }

            for (var i = 0; i <= bytesToRead; i++)
            {
                var left = (byte)(_buffer[byteOffset + i] << shift);
                var right = (byte)((_buffer[byteOffset + i + 1] &
                                    (i == bytesToRead + 1 ? 0xFF << (8 - bitLength % 8) : 0xFF)) >> (8 - shift));

                value[valueOffset + i] = (byte)(left | right);
            }
        }

        _bitOffset += bitLength;
        return bitLength;
    }

    public bool ReadBit()
    {
        if (_bitOffset >= _totalBits)
        {
            throw new InvalidOperationException("No more bits to read.");
        }

        var byteIndex = _bitOffset / 8;
        var bitIndex = _bitOffset % 8;

        // Extract the bit at the current BitOffset
        var bit = (_buffer[byteIndex] >> (7 - bitIndex)) & 1;

        // Increment the BitOffset
        _bitOffset++;

        return bit == 1;
    }

    public byte ReadByteFromBits(int bits)
    {
        Span<byte> bytes = stackalloc byte[sizeof(byte)];
        ReadBits(bytes, bits);
        var mask = (1 << bits) - 1;
        return (byte)((bytes[0] >> (sizeof(byte) * 8 - bits)) & mask);
    }

    public short ReadInt16FromBits(int bits, bool bigEndian = true)
    {
        Span<byte> bytes = stackalloc byte[sizeof(short)];
        ReadBits(bytes, bits);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            bytes.Reverse();
        }

        var mask = (1 << bits) - 1;
        return (short)((BinaryPrimitives.ReadInt16LittleEndian(bytes) >> (sizeof(short) * 8 - bits)) & mask);
    }

    public ushort ReadUInt16FromBits(int bits, bool bigEndian = true)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        ReadBits(bytes, bits);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            bytes.Reverse();
        }

        var mask = (1 << bits) - 1;
        return (ushort)((BinaryPrimitives.ReadUInt16LittleEndian(bytes) >> (sizeof(ushort) * 8 - bits)) & mask);
    }

    public int ReadInt32FromBits(int bits, bool bigEndian = true)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        ReadBits(bytes, bits);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            bytes.Reverse();
        }

        var mask = bits != 32 ? (1 << bits) - 1 : -1;
        return (BinaryPrimitives.ReadInt32LittleEndian(bytes) >> (sizeof(int) * 8 - bits)) & mask;
    }

    public long ReadInt64FromBits(int bits, bool bigEndian = true)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        ReadBits(bytes, bits);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            bytes.Reverse();
        }

        return BinaryPrimitives.ReadInt64LittleEndian(bytes) >> (sizeof(long) * 8 - bits);
    }

    public bool HasMoreBits(int requiredBits)
    {
        return _bitOffset + requiredBits <= _totalBits;
    }

    public void SkipBits(int v)
    {
        if (_bitOffset + v > _totalBits)
        {
            throw new InvalidOperationException($"Cannot skip {v} bits, only {_totalBits - _bitOffset} remain.");
        }

        _bitOffset += v;
    }
}