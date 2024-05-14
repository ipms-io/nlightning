namespace NLightning.Bolts.BOLT2.Validators;

using Payloads;

public static class TxRemoveOutputValidator
{
    public static void Validate(bool isInitiator, TxRemoveOutputPayload output, Func<ulong, bool> isSerialIdPresent)
    {
        if (isInitiator && (output.SerialId & 1) != 0) // Ensure even serial_id for initiator
        {
            throw new InvalidOperationException("SerialId has the wrong parity.");
        }

        if (!isSerialIdPresent(output.SerialId))
        {
            throw new InvalidOperationException("The serial_id does not correspond to a currently added output.");
        }
    }
}