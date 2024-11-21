namespace NLightning.Common.Tests.Utils;

public static class TestUtils
{
    public static byte[] GetBytes(string hex)
    {
        return Convert.FromHexString(hex.Replace("", string.Empty));
    }
}