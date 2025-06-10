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
    public const string Mainnet = "mainnet";
    public const string Testnet = "testnet";
    public const string Regtest = "regtest";
    public const string Signet = "signet";
}