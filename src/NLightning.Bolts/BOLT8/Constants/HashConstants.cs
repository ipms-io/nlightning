namespace NLightning.Bolts.BOLT8.Constants;

internal static class HashConstants
{
    /// <summary>
    /// A constant specifying the size in bytes of the hash function's output.
    /// </summary>
    public const int HASH_LEN = 32;

    /// <summary>
    /// A constant specifying the size in bytes that the hash function
    /// uses internally to divide its input for iterative processing.
    /// </summary>
    internal const int BLOCK_LEN = 64;
}