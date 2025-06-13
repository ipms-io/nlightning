using System.Diagnostics.CodeAnalysis;
using NLightning.Domain.Money;

namespace NLightning.Domain.Bitcoin.Transactions.Constants;

[ExcludeFromCodeCoverage]
public static class TransactionConstants
{
    public const uint CommitmentTransactionVersion = 2;
    public const uint HtlcTransactionVersion = 2;
    public const uint FundingTransactionVersion = 2;

    public const int CommitmentTransactionInputWeight = WeightConstants.WitnessHeader
                                                      + WeightConstants.MultisigWitnessWeight
                                                      + 4 * WeightConstants.P2WshInputWeight;

    public static readonly LightningMoney AnchorOutputAmount = LightningMoney.Satoshis(330);

    public const int TxIdLength = 32;

    public const int InitialCommitmentTransactionWeightNoAnchor = WeightConstants.WitnessHeader
                                                                + WeightConstants.MultisigWitnessWeight
                                                                + 4 * WeightConstants.P2WshInputWeight
                                                                + WeightConstants.P2WshOutputWeight
                                                                + WeightConstants.P2WpkhOutputWeight;

    public const int InitialCommitmentTransactionWeightWithAnchor = WeightConstants.WitnessHeader
                                                                  + WeightConstants.MultisigWitnessWeight
                                                                  + 4 * WeightConstants.P2WshInputWeight
                                                                  + 2 * WeightConstants.P2WshOutputWeight
                                                                  + 2 * WeightConstants.AnchorOutputWeight;
}