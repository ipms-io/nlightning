using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace NLightning.Common.BitUtils;

public unsafe class BitReader
{
    public int BitOffset { get; private set; } = 0;

    public readonly byte* Buffer;
    public readonly int TotalBits;

    public BitReader(byte[] buffer)
    {
        fixed (byte* p = buffer)
        {
            Buffer = p;
            TotalBits = buffer.Length * 8;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadBits(Span<byte> value, int bitLength)
    {
        // copy bytes to value 
        fixed (byte* p = value)
        {
            return ReadBits(p, 0, bitLength);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadBits(Span<byte> value, int valueOffset, int bitLength)
    {
        // copy bytes to value 
        fixed (byte* p = value)
        {
            return ReadBits(p, valueOffset, bitLength);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadBits(byte* value, int valueOffset, int bitLength)
    {
        var byteOffset = BitOffset / 8;
        var shift = (int)((uint)BitOffset % 8);

        if (shift == 0)
        {
            // copy bytes to value 
            Unsafe.CopyBlock(value + valueOffset, Buffer + byteOffset, (uint)(bitLength / 8 + (bitLength % 8 == 0 ? 0 : 1)));

            // mask extra bits 
            if (bitLength % 8 != 0)
            {
                value[valueOffset + bitLength / 8] &= (byte)(0xFF << (8 - bitLength % 8));
            }

            BitOffset += bitLength;

            return bitLength;
        }

        var bytesToRead = bitLength / 8 + (shift > 0 ? 1 : 0);
        for (var i = 0; i < bytesToRead; i++)
        {
            var left = (byte)(Buffer[byteOffset + i] << shift);
            var right = (byte)((Buffer[byteOffset + i + 1] & (i == bytesToRead - 1 ? 0xFF << (8 - bitLength % 8) : 0xFF)) >> (8 - shift));

            value[valueOffset + i] = (byte)(left | right);
        }

        BitOffset += bitLength;
        return bitLength;
    }

    public bool ReadBit()
    {
        if (BitOffset >= TotalBits)
        {
            throw new InvalidOperationException("No more bits to read.");
        }

        var byteIndex = BitOffset / 8;
        var bitIndex = BitOffset % 8;

        // Extract the bit at the current BitOffset
        var bit = (Buffer[byteIndex] >> (7 - bitIndex)) & 1;

        // Increment the BitOffset
        BitOffset++;

        return bit == 1;
    }

    public byte ReadByteFromBits(int bits)
    {
        var bytes = new byte[sizeof(byte)];
        ReadBits(bytes, bits);
        var mask = (1 << bits) - 1;
        return (byte)((bytes[0] >> (sizeof(byte) * 8 - bits)) & mask);
    }

    public short ReadInt16FromBits(int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(short)];
        ReadBits(bytes, bits);

        if ((bigEndian && System.BitConverter.IsLittleEndian) || (!bigEndian && !System.BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        var mask = (1 << bits) - 1;
        return (short)((BinaryPrimitives.ReadInt16LittleEndian(bytes) >> (sizeof(short) * 8 - bits)) & mask);
    }

    public int ReadInt32FromBits(int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(int)];
        ReadBits(bytes, bits);

        if ((bigEndian && System.BitConverter.IsLittleEndian) || (!bigEndian && !System.BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        var mask = bits != 32 ? (1 << bits) - 1 : -1;
        return (BinaryPrimitives.ReadInt32LittleEndian(bytes) >> (sizeof(int) * 8 - bits)) & mask;
    }

    public long ReadInt64FromBits(int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(long)];
        ReadBits(bytes, bits);

        if ((bigEndian && System.BitConverter.IsLittleEndian) || (!bigEndian && !System.BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        return BinaryPrimitives.ReadInt64LittleEndian(bytes) >> (sizeof(long) * 8 - bits);
    }

    public bool HasMoreBits(int requiredBits)
    {
        return BitOffset + requiredBits <= TotalBits;
    }

    public void SkipBits(int v)
    {
        BitOffset += v;
    }
}