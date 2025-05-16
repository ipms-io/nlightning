namespace NLightning.Domain.Serialization;

public interface IBitReader
{
    int ReadBits(Span<byte> value, int bitLength);
    bool ReadBit();
    byte ReadByteFromBits(int bits);
    short ReadInt16FromBits(int bits, bool bigEndian = true);
    ushort ReadUInt16FromBits(int bits, bool bigEndian = true);
    int ReadInt32FromBits(int bits, bool bigEndian = true);
    long ReadInt64FromBits(int bits, bool bigEndian = true);
    bool HasMoreBits(int requiredBits);
    void SkipBits(int bits);
}