namespace NLightning.Bolt11.Constants;

public static class TaggedFieldConstants
{
    public const short HashLength = 52;
    public const short PayeePubkeyLength = 53;

    /// <summary>
    /// The length of a single routing information entry in bits
    /// </summary>
    /// <remarks>
    /// The routing information entry is 264 + 64 + 32 + 32 + 16 = 408 bits long
    /// </remarks>
    public const int RoutingInfoLength = 408;
}