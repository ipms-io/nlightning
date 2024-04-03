namespace NLightning.Common.Constants;

using Types;

/// <summary>
/// Constants for the different chains.
/// </summary>
/// <remarks>
/// The chain constants are used to identify the chain in the network messages.
/// </remarks>
public static class ChainConstants
{
    #pragma warning disable format
    /// <summary>
    /// The main chain.
    /// </summary>
    public static readonly ChainHash Main = new([
        0x6f, 0xe2, 0x8c, 0x0a, 0xb6, 0xf1, 0xb3, 0x72,
        0xc1, 0xa6, 0xa2, 0x46, 0xae, 0x63, 0xf7, 0x4f,
        0x93, 0x1e, 0x83, 0x65, 0xe1, 0x5a, 0x08, 0x9c,
        0x68, 0xd6, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00
    ]);
    
    /// <summary>
    /// The testnet chain.
    /// </summary>
    public static readonly ChainHash Testnet = new([
        0x43, 0x49, 0x4f, 0x77, 0xd7, 0x8f, 0x26, 0x95,
        0x71, 0x08, 0xf4, 0xa3, 0x0f, 0xd9, 0xce, 0xc3,
        0xae, 0xba, 0x79, 0x97, 0x20, 0x84, 0xe9, 0x0e,
        0xad, 0x01, 0xea, 0x33, 0x09, 0x00, 0x00, 0x00
    ]);

    /// <summary>
    /// The regtest chain.
    /// </summary>
    public static readonly ChainHash Regtest = new([
        0x06, 0x22, 0x6e, 0x46, 0x11, 0x1a, 0x0b, 0x59,
        0xca, 0xaf, 0x12, 0x60, 0x43, 0xeb, 0x5b, 0xbf,
        0x28, 0xc3, 0x4f, 0x3a, 0x5e, 0x33, 0x2a, 0x1f,
        0xc7, 0xb2, 0xb7, 0x3c, 0xf1, 0x88, 0x91, 0x0f
    ]);
    #pragma warning restore format
}