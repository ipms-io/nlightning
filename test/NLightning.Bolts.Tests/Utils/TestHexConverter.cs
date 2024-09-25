namespace NLightning.Bolts.Tests.Utils;

public static class TestHexConverter
{
    public static string ToHexString(this byte[] bytes)
    {
        return "0x" + BitConverter.ToString(bytes).Replace("-", "");
    }

    public static byte[] ToByteArray(this string hex)
    {
        return Convert.FromHexString(hex.Replace("0x", string.Empty));
    }
}