using System.Diagnostics.CodeAnalysis;

namespace NLightning.Domain.Protocol.Constants;

[ExcludeFromCodeCoverage]
public static class InteractiveTransactionConstants
{
    public const int MaxInputsAllowed = 252;
    public const uint MaxSequence = 0xFFFFFFFD;
    public const int MaxOutputsAllowed = 252;
    public const ulong MaxMoney = 2_100_000_000_000_000;
    public const ulong MaxStandardTxWeight = 400_000;
}