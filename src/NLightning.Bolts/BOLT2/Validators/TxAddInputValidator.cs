namespace NLightning.Bolts.BOLT2.Validators;

using Constants;
using Payloads;

public static class TxAddInputValidator
{
    public static async void Validate(bool isInitiator, TxAddInputPayload input, int currentInputCount, Func<byte[], Task<bool>> isValidPrevTx, Func<byte[], uint, bool> isUniqueInput, Func<ulong, bool> isSerialIdUnique)
    {
        if (isInitiator && (input.SerialId & 1) != 0) // Ensure even serial_id for initiator
        {
            throw new InvalidOperationException("SerialId has the wrong parity.");
        }

        if (!isSerialIdUnique(input.SerialId))
        {
            throw new InvalidOperationException("SerialId is already included in the transaction.");
        }

        if (!await isValidPrevTx(input.PrevTx))
        {
            throw new InvalidOperationException("PrevTx is not a valid transaction.");
        }

        if (input.PrevTxVout >= GetOutputCount(input.PrevTx))
        {
            throw new InvalidOperationException("PrevTxVout is greater than or equal to the number of outputs on PrevTx.");
        }

        if (!IsScriptPubKeyValid(input.PrevTx, input.PrevTxVout))
        {
            throw new InvalidOperationException("ScriptPubKey of the PrevTxVout output is not valid.");
        }

        if (!isUniqueInput(input.PrevTx, input.PrevTxVout))
        {
            throw new InvalidOperationException("The PrevTx and PrevTxVout are identical to a previously added input.");
        }

        if (currentInputCount >= InteractiveTransactionContants.MAX_INPUTS_ALLOWED)
        {
            throw new InvalidOperationException($"Cannot receive more than {InteractiveTransactionContants.MAX_INPUTS_ALLOWED} tx_add_input messages during this negotiation.");
        }
    }

    private static uint GetOutputCount(byte[] prevTx)
    {
        _ = prevTx;
        // TODO: Implement logic to parse prevTx and return the output count
        return 1;
    }

    private static bool IsScriptPubKeyValid(byte[] prevTx, uint prevTxVout)
    {
        _ = prevTx;
        _ = prevTxVout;
        // TODO: Implement logic to parse prevTx and check the scriptPubKey of the given output
        return true;
    }
}