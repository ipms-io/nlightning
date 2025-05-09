namespace NLightning.Infrastructure.Serialization;

using Common.BitUtils;
using Domain.Serialization;

public class EndianConverterAdapter : IEndianConverter
{
    public byte[] GetBytesBigEndian(ushort value) => EndianBitConverter.GetBytesBigEndian(value);
    public byte[] GetBytesBigEndian(uint value) => EndianBitConverter.GetBytesBigEndian(value);
    public byte[] GetBytesBigEndian(ulong value) => EndianBitConverter.GetBytesBigEndian(value);
    public byte[] GetBytesBigEndian(long value) => EndianBitConverter.GetBytesBigEndian(value);
    
    public ushort ToUInt16BigEndian(byte[] bytes) => EndianBitConverter.ToUInt16BigEndian(bytes);
    public uint ToUInt32BigEndian(byte[] bytes) => EndianBitConverter.ToUInt32BigEndian(bytes);
    public long ToInt64BigEndian(byte[] tlvValue) => EndianBitConverter.ToInt64BigEndian(tlvValue);
    public ulong ToUInt64BigEndian(byte[] bytes) => EndianBitConverter.ToUInt64BigEndian(bytes);
}