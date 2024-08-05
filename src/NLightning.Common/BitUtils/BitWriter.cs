using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace NLightning.Common.BitUtils;

public unsafe class BitWriter
{
    public int BitOffset { get; private set; } = 0;

    public readonly byte* Buffer;
    public readonly int TotalBits;

    public BitWriter(int totalBits)
    {
        Buffer = (byte*)Unsafe.AsPointer(ref totalBits);
        TotalBits = totalBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBits(ReadOnlySpan<byte> value, int bitLength)
    {
        // copy bytes from value 
        fixed (byte* p = value)
        {
            WriteBits(p, 0, bitLength);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBits(ReadOnlySpan<byte> value, int valueOffset, int bitLength)
    {
        // copy bytes from value 
        fixed (byte* p = value)
        {
            WriteBits(p, valueOffset, bitLength);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBits(byte* value, int valueOffset, int bitLength)
    {
        var byteOffset = BitOffset / 8;
        var shift = (int)((uint)BitOffset % 8);

        if (shift == 0)
        {
            // copy bytes from value 
            Unsafe.CopyBlock(Buffer + byteOffset, value + valueOffset, (uint)(bitLength / 8 + (bitLength % 8 == 0 ? 0 : 1)));

            BitOffset += bitLength;

            return;
        }

        var bytesToWrite = bitLength / 8 + (shift > 0 ? 1 : 0);
        for (var i = 0; i < bytesToWrite; i++)
        {
            var left = (byte)((value[valueOffset + i] >> shift) & 0xFF);
            var right = (byte)((value[valueOffset + i] << (8 - shift)) & 0xFF);

            Buffer[byteOffset + i] |= left;
            Buffer[byteOffset + i + 1] = right;
        }

        BitOffset += bitLength;
    }

    public void WriteBit(bool bit)
    {
        if (BitOffset >= TotalBits)
        {
            throw new InvalidOperationException("No more bits to write.");
        }

        var byteIndex = BitOffset / 8;
        var bitIndex = BitOffset % 8;

        if (bit)
        {
            Buffer[byteIndex] |= (byte)(1 << (7 - bitIndex));
        }
        else
        {
            Buffer[byteIndex] &= (byte)~(1 << (7 - bitIndex));
        }

        BitOffset++;
    }

    public void WriteByteAsBits(byte value, int bits)
    {
        var bytes = new byte[] { value };
        WriteBits(bytes, bits);
    }

    public void WriteInt16AsBits(short value, int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(short)];
        if (bigEndian)
        {
            BinaryPrimitives.WriteInt16BigEndian(bytes, value);
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian(bytes, value);
        }

        WriteBits(bytes, bits);
    }

    public void WriteInt32AsBits(int value, int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(int)];
        if (bigEndian)
        {
            BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        }

        WriteBits(bytes, bits);
    }

    public void WriteInt64AsBits(long value, int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(long)];
        if (bigEndian)
        {
            BinaryPrimitives.WriteInt64BigEndian(bytes, value);
        }
        else
        {
            BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        }

        WriteBits(bytes, bits);
    }

    public bool HasMoreBits(int requiredBits)
    {
        return BitOffset + requiredBits <= TotalBits;
    }

    public void SkipBits(int v)
    {
        BitOffset += v;
    }

    public byte[] ToArray()
    {
        var bytes = new byte[(TotalBits + 7) / 8];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Buffer[i];
        }

        return bytes;
    }
}