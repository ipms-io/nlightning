namespace NLightning.Domain.Utils;

public interface IBitWriter : IDisposable
{
    int TotalBits { get; }
    void GrowByBits(int additionalBits);
    void WriteBits(ReadOnlySpan<byte> value, int bitLength);
    void WriteBits(ReadOnlySpan<byte> value, int valueOffset, int bitLength);
    void WriteBit(bool bit);
    void WriteInt16AsBits(short value, int bits, bool bigEndian = true);
    void WriteUInt16AsBits(ushort value, int bits, bool bigEndian = true);
    void WriteInt32AsBits(int value, int bits, bool bigEndian = true);
    void WriteInt64AsBits(long value, int bits, bool bigEndian = true);
    bool HasMoreBits(int requiredBits);
    void SkipBits(int v);
    byte[] ToArray();
}