namespace NLightning.Bolts.Tests.BOLT8.Utils;

public static class TestUtils
{
    public static byte[] GetBytes(string hex)
    {
        hex = hex.Replace("0x", string.Empty);
        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }
}
