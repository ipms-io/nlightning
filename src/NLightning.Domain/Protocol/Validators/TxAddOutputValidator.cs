namespace NLightning.Domain.Protocol.Validators;

using Constants;
using Payloads;
using ValueObjects;

public static class TxAddOutputValidator
{
    public static void Validate(bool isInitiator, TxAddOutputPayload output, int currentOutputCount,
                                Func<ulong, bool> isSerialIdUnique, Func<byte[], bool> isStandardScript,
                                LightningMoney dustLimit)
    {
        if (isInitiator && (output.SerialId & 1) != 0) // Ensure even serial_id for initiator
        {
            throw new InvalidOperationException("SerialId has the wrong parity.");
        }

        if (!isSerialIdUnique(output.SerialId))
        {
            throw new InvalidOperationException("SerialId is already included in the transaction.");
        }

        if (currentOutputCount >= InteractiveTransactionConstants.MAX_OUTPUTS_ALLOWED)
        {
            throw new InvalidOperationException($"Cannot receive more than {InteractiveTransactionConstants.MAX_OUTPUTS_ALLOWED} tx_add_output messages during this negotiation.");
        }

        if (output.Amount < dustLimit)
        {
            throw new InvalidOperationException("The sats amount is less than the dust_limit.");
        }

        if (output.Amount > InteractiveTransactionConstants.MAX_MONEY)
        {
            throw new InvalidOperationException("The sats amount is greater than the maximum allowed (MAX_MONEY).");
        }

        if (!isStandardScript(output.Script))
        {
            throw new InvalidOperationException("The script is non-standard.");
        }
    }
}