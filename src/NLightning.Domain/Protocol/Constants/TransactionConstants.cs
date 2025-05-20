using System.Diagnostics.CodeAnalysis;

namespace NLightning.Domain.Protocol.Constants;

[ExcludeFromCodeCoverage]
public static class TransactionConstants
{
    public const uint COMMITMENT_TRANSACTION_VERSION = 2;
    public const uint HTLC_TRANSACTION_VERSION = 2;
    public const uint FUNDING_TRANSACTION_VERSION = 2;

    public const int COMMITMENT_TRANSACTION_INPUT_WEIGHT = WeightConstants.WITNESS_HEADER
                                                         + WeightConstants.MULTISIG_WITNESS_WEIGHT
                                                         + (4 * WeightConstants.P2WSH_INTPUT_WEIGHT);

    public const int TX_ID_LENGTH = 32;
}