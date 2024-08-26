using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NLightning.Common.BitUtils;

public unsafe class BitReader : IDisposable
{
    private int _bitOffset;
    private byte* _buffer;
    private GCHandle _handle;

    private readonly int _totalBits;

    public BitReader(byte[] buffer)
    {
        _handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        _buffer = (byte*)_handle.AddrOfPinnedObject();
        _totalBits = buffer.Length * 8;
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
    private int ReadBits(byte* value, int valueOffset, int bitLength)
    {
        var byteOffset = _bitOffset / 8;
        var shift = (int)((uint)_bitOffset % 8);

        if (shift == 0)
        {
            // copy bytes to value 
            Unsafe.CopyBlock(value + valueOffset, _buffer + byteOffset, (uint)(bitLength / 8 + (bitLength % 8 == 0 ? 0 : 1)));

            // mask extra bits 
            if (bitLength % 8 != 0)
            {
                value[valueOffset + bitLength / 8] &= (byte)(0xFF << (8 - bitLength % 8));
            }

            _bitOffset += bitLength;

            return bitLength;
        }

        var bytesToRead = bitLength / 8 + (shift > 0 ? 1 : 0);
        for (var i = 0; i < bytesToRead; i++)
        {
            var left = (byte)(_buffer[byteOffset + i] << shift);
            var right = (byte)((_buffer[byteOffset + i + 1] & (i == bytesToRead - 1 ? 0xFF << (8 - bitLength % 8) : 0xFF)) >> (8 - shift));

            value[valueOffset + i] = (byte)(left | right);
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
        var bytes = new byte[sizeof(byte)];
        ReadBits(bytes, bits);
        var mask = (1 << bits) - 1;
        return (byte)((bytes[0] >> (sizeof(byte) * 8 - bits)) & mask);
    }

    public short ReadInt16FromBits(int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(short)];
        ReadBits(bytes, bits);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        var mask = (1 << bits) - 1;
        return (short)((BinaryPrimitives.ReadInt16LittleEndian(bytes) >> (sizeof(short) * 8 - bits)) & mask);
    }

    public ushort ReadUInt16FromBits(int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(ushort)];
        ReadBits(bytes, bits);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        var mask = (1 << bits) - 1;
        return (ushort)((BinaryPrimitives.ReadUInt16LittleEndian(bytes) >> (sizeof(ushort) * 8 - bits)) & mask);
    }

    public int ReadInt32FromBits(int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(int)];
        ReadBits(bytes, bits);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
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

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        return BinaryPrimitives.ReadInt64LittleEndian(bytes) >> (sizeof(long) * 8 - bits);
    }

    public bool HasMoreBits(int requiredBits)
    {
        return _bitOffset + requiredBits <= _totalBits;
    }

    public void SkipBits(int v)
    {
        _bitOffset += v;
    }

    private void ReleaseUnmanagedResources()
    {
        if (_handle.IsAllocated)
        {
            _handle.Free();
        }

        _buffer = null;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~BitReader()
    {
        ReleaseUnmanagedResources();
    }
}