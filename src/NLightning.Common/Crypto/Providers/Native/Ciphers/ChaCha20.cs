namespace NLightning.Common.Crypto.Providers.Native.Ciphers;

public static class ChaCha20
{
    public static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
    {
        a += b; d = RotateLeft(d ^ a, 16);
        c += d; b = RotateLeft(b ^ c, 12);
        a += b; d = RotateLeft(d ^ a, 8);
        c += d; b = RotateLeft(b ^ c, 7);
    }

    private static uint RotateLeft(uint value, int offset)
    {
        return (value << offset) | (value >> (32 - offset));
    }
}