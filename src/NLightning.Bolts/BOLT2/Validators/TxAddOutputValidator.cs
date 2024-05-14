namespace NLightning.Bolts.BOLT2.Validators;

using Constants;
using Payloads;

public static class TxAddOutputValidator
{
    public static void Validate(bool isInitiator, TxAddOutputPayload output, int currentOutputCount, Func<ulong, bool> isSerialIdUnique, Func<byte[], bool> isStandardScript, ulong dustLimit)
    {
        if (!isSerialIdUnique(output.SerialId))
        {
            throw new InvalidOperationException("SerialId is already included in the transaction.");
        }

        if (isInitiator && (output.SerialId & 1) != 0) // Ensure even serial_id for initiator
        {
            throw new InvalidOperationException("SerialId has the wrong parity.");
        }

        if (currentOutputCount >= InteractiveTransactionContants.MAX_OUTPUTS_ALLOWED)
        {
            throw new InvalidOperationException($"Cannot receive more than {InteractiveTransactionContants.MAX_OUTPUTS_ALLOWED} tx_add_output messages during this negotiation.");
        }

        if (output.Sats < dustLimit)
        {
            throw new InvalidOperationException("The sats amount is less than the dust_limit.");
        }

        if (output.Sats > InteractiveTransactionContants.MAX_MONEY)
        {
            throw new InvalidOperationException("The sats amount is greater than the maximum allowed (MAX_MONEY).");
        }

        if (!isStandardScript(output.Script))
        {
            throw new InvalidOperationException("The script is non-standard.");
        }
    }
}