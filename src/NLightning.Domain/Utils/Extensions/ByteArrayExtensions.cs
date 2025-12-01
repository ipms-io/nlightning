namespace NLightning.Domain.Utils.Extensions;

public static class ByteArrayExtensions
{
    public static int GetByteArrayHashCode(this byte[] bytes)
    {
        if (bytes.Length == 0)
            return 0;

        unchecked
        {
            return bytes.Aggregate(24, (current, t) => (current * 69) ^ t);
        }
    }
}