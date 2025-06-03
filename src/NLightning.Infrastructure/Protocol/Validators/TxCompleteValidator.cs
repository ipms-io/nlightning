namespace NLightning.Infrastructure.Protocol.Validators;

using Domain.Protocol.Constants;

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

        if (currentInputCount > InteractiveTransactionConstants.MaxInputsAllowed)
        {
            throw new InvalidOperationException($"There are more than {InteractiveTransactionConstants.MaxInputsAllowed} inputs.");
        }

        if (currentOutputCount > InteractiveTransactionConstants.MaxOutputsAllowed)
        {
            throw new InvalidOperationException($"There are more than {InteractiveTransactionConstants.MaxOutputsAllowed} outputs.");
        }

        if (estimatedTxWeight > InteractiveTransactionConstants.MaxStandardTxWeight)
        {
            throw new InvalidOperationException($"The estimated weight of the transaction is greater than {InteractiveTransactionConstants.MaxStandardTxWeight}.");
        }
    }
}