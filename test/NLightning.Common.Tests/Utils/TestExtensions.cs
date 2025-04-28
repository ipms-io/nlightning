namespace NLightning.Common.Tests.Utils;

public static class TestExtensions
{
    public static byte[] GetBytes(this string hex)
    {
        return Convert.FromHexString(hex.Replace("0x", string.Empty));
    }
}