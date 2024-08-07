using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NLightning.Common.BitUtils;

public unsafe class BitWriter : IDisposable
{
    private int _bitOffset;

    private byte* _buffer;
    private GCHandle _handle;
    private byte[] _managedBuffer;
    private int _totalBits;

    public BitWriter(int totalBits)
    {
        var totalBytes = (totalBits + 7) / 8;
        _managedBuffer = new byte[totalBytes];
        _handle = GCHandle.Alloc(_managedBuffer, GCHandleType.Pinned);
        _buffer = (byte*)_handle.AddrOfPinnedObject();
        _totalBits = totalBits;
    }

    public void GrowByBits(int additionalBits)
    {
        var requiredBits = _bitOffset + additionalBits;
        var requiredBytes = (requiredBits + 7) / 8;
        if (requiredBytes <= _managedBuffer.Length)
        {
            return;
        }

        var newCapacity = Math.Max(_managedBuffer.Length * 2, requiredBytes);
        Array.Resize(ref _managedBuffer, newCapacity);
        _handle.Free();
        _handle = GCHandle.Alloc(_managedBuffer, GCHandleType.Pinned);
        _buffer = (byte*)_handle.AddrOfPinnedObject();
        _totalBits = requiredBits;
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
        var byteOffset = _bitOffset / 8;
        var shift = (int)((uint)_bitOffset % 8);

        if (shift == 0)
        {
            // copy bytes from value 
            Unsafe.CopyBlock(_buffer + byteOffset, value + valueOffset, (uint)(bitLength / 8 + (bitLength % 8 == 0 ? 0 : 1)));

            _bitOffset += bitLength;

            return;
        }

        var bytesToWrite = bitLength / 8 + (shift > 0 ? 1 : 0);
        for (var i = 0; i < bytesToWrite; i++)
        {
            var left = (byte)((value[valueOffset + i] >> shift) & 0xFF);
            var right = (byte)((value[valueOffset + i] << (8 - shift)) & 0xFF);

            _buffer[byteOffset + i] |= left;
            _buffer[byteOffset + i + 1] = right;
        }

        _bitOffset += bitLength;
    }

    public void WriteBit(bool bit)
    {
        if (_bitOffset >= _totalBits)
        {
            throw new InvalidOperationException("No more bits to write.");
        }

        var byteIndex = _bitOffset / 8;
        var bitIndex = _bitOffset % 8;

        if (bit)
        {
            _buffer[byteIndex] |= (byte)(1 << (7 - bitIndex));
        }
        else
        {
            _buffer[byteIndex] &= (byte)~(1 << (7 - bitIndex));
        }

        _bitOffset++;
    }

    public void WriteByteAsBits(byte value, int bits)
    {
        var mask = (1 >> bits) - 1;
        var bytes = new[] { (byte)((value << (sizeof(byte) * 8 - bits)) & mask) };
        WriteBits(bytes, bits);
    }

    public void WriteInt16AsBits(short value, int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(short)];
        var mask = (1 >> bits) - 1;
        BinaryPrimitives.WriteInt16LittleEndian(bytes, (short)((value << (sizeof(short) * 8 - bits)) & mask));

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        WriteBits(bytes, bits);
    }

    public void WriteUInt16AsBits(ushort value, int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(ushort)];
        var mask = (1 >> bits) - 1;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, (ushort)((value << (sizeof(ushort) * 8 - bits)) & mask));

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        WriteBits(bytes, bits);
    }

    public void WriteInt32AsBits(int value, int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(int)];
        var mask = bits != 32 ? (1 >> bits) - 1 : -1;
        BinaryPrimitives.WriteInt32LittleEndian(bytes, (value << (sizeof(int) * 8 - bits)) & mask);

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        WriteBits(bytes, bits);
    }

    public void WriteInt64AsBits(long value, int bits, bool bigEndian = true)
    {
        var bytes = new byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value << (sizeof(long) * 8 - bits));

        if ((bigEndian && BitConverter.IsLittleEndian) || (!bigEndian && !BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        WriteBits(bytes, bits);
    }

    public bool HasMoreBits(int requiredBits)
    {
        return _bitOffset + requiredBits <= _totalBits;
    }

    public void SkipBits(int v)
    {
        _bitOffset += v;
    }

    public byte[] ToArray()
    {
        var bytes = new byte[(_totalBits + 7) / 8];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = _buffer[i];
        }

        return bytes;
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

    ~BitWriter()
    {
        ReleaseUnmanagedResources();
    }
}