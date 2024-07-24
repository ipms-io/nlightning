namespace NLightning.Common.Utils;

public static class BitUtils
{
    public static byte[] ConvertBits(byte[] input, int fromBits, int toBits, bool pad = true)
    {
        var acc = 0;
        var bits = 0;
        var ret = new List<byte>();
        var maxV = (1 << toBits) - 1;
        var maxAcc = (1 << (fromBits + toBits - 1)) - 1;

        foreach (var value in input)
        {
            if (value < 0 || value >> fromBits != 0)
            {
                throw new ArgumentException("Invalid data value for given bit width.");
            }

            acc = ((acc << fromBits) | value) & maxAcc;
            bits += fromBits;

            while (bits >= toBits)
            {
                bits -= toBits;
                ret.Add((byte)((acc >> bits) & maxV));
            }
        }

        if (pad && bits > 0)
        {
            ret.Add((byte)((acc << (toBits - bits)) & maxV));
        }
        else if (!pad && (bits >= fromBits || ((acc << (toBits - bits)) & maxV) != 0))
        {
            throw new ArgumentException("Cannot convert bits without padding and with remaining bits.");
        }

        return [.. ret];
    }

    public static byte[] ReadBytesFromBits(byte[] data)//, int offset, int count)
    {
        var bits = ConvertBits(data, 5, 8, false);
        return bits;
    }
}