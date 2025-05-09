namespace NLightning.Infrastructure.Crypto.Constants;

public class HashConstants
{
    /// <summary>
    /// A constant specifying the size in bytes of the SHA256 hash function's output.
    /// </summary>
    public const int SHA256_HASH_LEN = 32;

    /// <summary>
    /// A constant specifying the size in bytes that the SHA256 hash function
    /// uses internally to divide its input for iterative processing.
    /// </summary>
    internal const int SHA256_BLOCK_LEN = 64;

    /// <summary>
    /// A constant specifying the size in bytes of the Ripemd160 hash function's output.
    /// </summary>
    public const int RIPEMD160_HASH_LEN = 20;
}