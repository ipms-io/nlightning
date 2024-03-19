namespace NLightning.Bolts.Tests.BOLT8.Utils;

public static class TestUtils
{
    public static byte[] GetBytes(string hex)
    {
        return Convert.FromHexString(hex.Replace("0x", string.Empty));
    }
}
