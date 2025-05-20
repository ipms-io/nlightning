using System.Diagnostics.CodeAnalysis;

namespace NLightning.Domain.Protocol.Constants;

/// <summary>
/// Constants for the different networks.
/// </summary>
/// <remarks>
/// The network constants are used to identify the network name.
/// </remarks>
[ExcludeFromCodeCoverage]
public static class NetworkConstants
{
    public const string MAINNET = "mainnet";
    public const string TESTNET = "testnet";
    public const string REGTEST = "regtest";
    public const string SIGNET = "signet";
}