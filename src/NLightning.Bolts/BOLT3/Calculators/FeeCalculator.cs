namespace NLightning.Bolts.BOLT3.Calculators;

using Common.Interfaces;
using Common.Managers;

public class FeeCalculator
{
    // These constants match the BOLT3 spec
    private const int COMMITMENT_WEIGHT_NO_ANCHORS = 724;  // 500 + 224 (base + witness)
    private const int COMMITMENT_WEIGHT_WITH_ANCHORS = 1124;  // 900 + 224 (base + witness)
    private const int HTLC_TIMEOUT_WEIGHT_NO_ANCHORS = 663;
    private const int HTLC_TIMEOUT_WEIGHT_WITH_ANCHORS = 666;
    private const int HTLC_SUCCESS_WEIGHT_NO_ANCHORS = 703;
    private const int HTLC_SUCCESS_WEIGHT_WITH_ANCHORS = 706;
    private const int HTLC_OUTPUT_WEIGHT = 172;

    // Funding transaction components from spec
    private const int TRANSACTION_FIELDS = 10;  // version, input count, output count, locktime
    private const int SEGWIT_FIELDS = 2;  // marker + flag
    private const int FUNDING_OUTPUT_SIZE = 43;  // value(8) + var_int(1) + p2wsh script(34)

    private readonly IFeeService _feeService;

    public const int SEGWIT_OUTPUT_BASE_SIZE = 9;  // value(8) + var_int(1)
    public const int SEGWIT_INPUT_SIZE = 41;  // prevout(36) + var_int(1) + empty script_sig(0) + sequence(4)

    public FeeCalculator(IFeeService feeService)
    {
        _feeService = feeService;
    }

    public ulong GetCurrentEstimatedFeePerKw()
    {
        return _feeService.GetCachedFeeRatePerKw();
    }

    public ulong CalculateCommitmentTransactionFee(int numUntrimmedHtlcs)
    {
        var baseWeight = ConfigManager.Instance.IsOptionAnchorOutput ? COMMITMENT_WEIGHT_WITH_ANCHORS : COMMITMENT_WEIGHT_NO_ANCHORS;
        var totalWeight = baseWeight + HTLC_OUTPUT_WEIGHT * numUntrimmedHtlcs;
        return GetCurrentEstimatedFeePerKw() * (ulong)totalWeight / 1000;
    }

    public ulong CalculateHtlcTimeoutTransactionFee()
    {
        var weight = ConfigManager.Instance.IsOptionAnchorOutput ? HTLC_TIMEOUT_WEIGHT_WITH_ANCHORS : HTLC_TIMEOUT_WEIGHT_NO_ANCHORS;
        return GetCurrentEstimatedFeePerKw() * (ulong)weight / 1000;
    }

    public ulong CalculateHtlcSuccessTransactionFee()
    {
        var weight = ConfigManager.Instance.IsOptionAnchorOutput ? HTLC_SUCCESS_WEIGHT_WITH_ANCHORS : HTLC_SUCCESS_WEIGHT_NO_ANCHORS;
        return GetCurrentEstimatedFeePerKw() * (ulong)weight / 1000;
    }

    /// <summary>
    /// Calculates funding transaction weight per BOLT3 spec
    /// </summary>
    public int CalculateFundingTransactionWeight(int inputSize, int numNonFundingOutputs, int scriptLengthSum,
                                                 int witnessSize)
    {
        // funding_transaction = 43 + num_inputs * 41 + num_outputs * 9 + sum(scriptlen)
        var nonWitnessSize = FUNDING_OUTPUT_SIZE +
                             inputSize +
                             (numNonFundingOutputs * SEGWIT_OUTPUT_BASE_SIZE) +
                             scriptLengthSum;

        // weight = 4 * (funding_transaction + transaction_fields) + segwit_fields + witness_weight
        return 4 * (nonWitnessSize + TRANSACTION_FIELDS) + SEGWIT_FIELDS + witnessSize;
    }

    /// <summary>
    /// Calculates funding transaction fee
    /// </summary>
    public ulong CalculateFundingTransactionFee(int inputSize, int numNonFundingOutputs, int scriptLengthSum,
                                                int witnessSize)
    {
        var weight = CalculateFundingTransactionWeight(inputSize, numNonFundingOutputs, scriptLengthSum, witnessSize);
        var feeRate = GetCurrentEstimatedFeePerKw();

        return (feeRate * (ulong)weight) / 1000;
    }
}