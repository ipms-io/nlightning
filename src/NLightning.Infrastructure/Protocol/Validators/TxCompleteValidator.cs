namespace NLightning.Domain.Protocol.Validators;

using Constants;

public static class TxCompleteValidator
{
    public static void Validate(ulong peerInputSatoshis, ulong peerOutputSatoshis, ulong peerFundingOutput, ulong agreedFeerate,
                                ulong peerPaidFeerate, int currentInputCount, int currentOutputCount, ulong estimatedTxWeight)
    {
        if (peerInputSatoshis < peerOutputSatoshis + peerFundingOutput)
        {
            throw new InvalidOperationException("The peer's total input satoshis is less than their outputs.");
        }

        if (peerPaidFeerate < agreedFeerate)
        {
            throw new InvalidOperationException("The peer's paid feerate does not meet or exceed the agreed feerate.");
        }

        if (currentInputCount > InteractiveTransactionConstants.MAX_INPUTS_ALLOWED)
        {
            throw new InvalidOperationException($"There are more than {InteractiveTransactionConstants.MAX_INPUTS_ALLOWED} inputs.");
        }

        if (currentOutputCount > InteractiveTransactionConstants.MAX_OUTPUTS_ALLOWED)
        {
            throw new InvalidOperationException($"There are more than {InteractiveTransactionConstants.MAX_OUTPUTS_ALLOWED} outputs.");
        }

        if (estimatedTxWeight > InteractiveTransactionConstants.MAX_STANDARD_TX_WEIGHT)
        {
            throw new InvalidOperationException($"The estimated weight of the transaction is greater than {InteractiveTransactionConstants.MAX_STANDARD_TX_WEIGHT}.");
        }
    }
}