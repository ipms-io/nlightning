namespace NLightning.Bolt11.Constants;

public static class TaggedFieldConstants
{
    public const short HASH_LENGTH = 52;
    public const short PAYEE_PUBKEY_LENGTH = 53;

    /// <summary>
    /// The length of a single routing information entry in bits
    /// </summary>
    /// <remarks>
    /// The routing information entry is 264 + 64 + 32 + 32 + 16 = 408 bits long
    /// </remarks>
    public const int ROUTING_INFO_LENGTH = 408;
}