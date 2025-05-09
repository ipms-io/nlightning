namespace NLightning.Domain.Serialization;

public interface IEndianConverter
{
    byte[] GetBytesBigEndian(ushort value);
    byte[] GetBytesBigEndian(uint value);
    byte[] GetBytesBigEndian(long value);
    byte[] GetBytesBigEndian(ulong value);
    
    ushort ToUInt16BigEndian(byte[] bytes);
    uint ToUInt32BigEndian(byte[] bytes);
    long ToInt64BigEndian(byte[] tlvValue);
    ulong ToUInt64BigEndian(byte[] bytes);
}