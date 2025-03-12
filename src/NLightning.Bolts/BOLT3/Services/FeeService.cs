using NLightning.Common.Managers;

namespace NLightning.Bolts.BOLT3.Services;

public class FeeService
{
    private const int COMMITMENT_WEIGHT_NO_ANCHORS = 724;
    private const int COMMITMENT_WEIGHT_WITH_ANCHORS = 1124;
    private const int HTLC_TIMEOUT_WEIGHT_NO_ANCHORS = 663;
    private const int HTLC_TIMEOUT_WEIGHT_WITH_ANCHORS = 666;
    private const int HTLC_SUCCESS_WEIGHT_NO_ANCHORS = 703;
    private const int HTLC_SUCCESS_WEIGHT_WITH_ANCHORS = 706;
    private const int HTLC_OUTPUT_WEIGHT = 172;

    public ulong GetCurrentEstimatedFeePerKw()
    {
        // TODO: Feed this from fees from web based service
        return 1000;
    }

    public ulong CalculateCommitmentTransactionFee(ulong feeratePerKw, int numUntrimmedHtlcs)
    {
        var baseWeight = ConfigManager.Instance.IsOptionAnchorOutput ? COMMITMENT_WEIGHT_WITH_ANCHORS : COMMITMENT_WEIGHT_NO_ANCHORS;
        var totalWeight = baseWeight + HTLC_OUTPUT_WEIGHT * numUntrimmedHtlcs;
        return feeratePerKw * (ulong)totalWeight / 1000;
    }

    public ulong CalculateHtlcTimeoutTransactionFee(ulong feeratePerKw)
    {
        var weight = ConfigManager.Instance.IsOptionAnchorOutput ? HTLC_TIMEOUT_WEIGHT_WITH_ANCHORS : HTLC_TIMEOUT_WEIGHT_NO_ANCHORS;
        return feeratePerKw * (ulong)weight / 1000;
    }

    public ulong CalculateHtlcSuccessTransactionFee(ulong feeratePerKw)
    {
        var weight = ConfigManager.Instance.IsOptionAnchorOutput ? HTLC_SUCCESS_WEIGHT_WITH_ANCHORS : HTLC_SUCCESS_WEIGHT_NO_ANCHORS;
        return feeratePerKw * (ulong)weight / 1000;
    }
}