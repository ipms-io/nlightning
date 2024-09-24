namespace NLightning.Common.Crypto.Hashes;

/// <summary>
/// RIPEMD-160 hash function abstraction
/// </summary>
internal static class Ripemd160
{
    /// <summary>
    /// Hashes data.
    /// </summary>
    internal static byte[] Hash(ReadOnlySpan<byte> data)
    {
        return NBitcoin.Crypto.Hashes.RIPEMD160(data.ToArray());
    }
}