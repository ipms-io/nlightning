namespace NLightning.Common.Utils;

public static class BitUtils
{
    public static byte[] ConvertBits(ReadOnlySpan<byte> data, int fromBits, int toBits, bool pad = true)
    {
        var num = 0;
        var num2 = 0;
        var num3 = (1 << toBits) - 1;
        var list = new List<byte>(64);
        var readOnlySpan = data;
        for (var i = 0; i < readOnlySpan.Length; i++)
        {
            var b = readOnlySpan[i];
            if (b >> fromBits > 0)
            {
                throw new FormatException("Invalid Bech32 string");
            }

            num = (num << fromBits) | b;
            num2 += fromBits;
            while (num2 >= toBits)
            {
                num2 -= toBits;
                list.Add((byte)((num >> num2) & num3));
            }
        }

        if (pad)
        {
            if (num2 > 0)
            {
                list.Add((byte)((num << toBits - num2) & num3));
            }
        }
        else if (num2 >= fromBits || (byte)((num << toBits - num2) & num3) != 0)
        {
            throw new FormatException("Invalid Bech32 string");
        }

        return [.. list];
    }
}