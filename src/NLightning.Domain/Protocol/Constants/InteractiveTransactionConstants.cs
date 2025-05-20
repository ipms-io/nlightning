using System.Diagnostics.CodeAnalysis;

namespace NLightning.Domain.Protocol.Constants;

[ExcludeFromCodeCoverage]
public static class InteractiveTransactionConstants
{
    public const int MAX_INPUTS_ALLOWED = 252;
    public const uint MAX_SEQUENCE = 0xFFFFFFFD;
    public const int MAX_OUTPUTS_ALLOWED = 252;
    public const ulong MAX_MONEY = 2_100_000_000_000_000;
    public const ulong MAX_STANDARD_TX_WEIGHT = 400_000;
}