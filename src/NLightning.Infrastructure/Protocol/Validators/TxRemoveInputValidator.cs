namespace NLightning.Infrastructure.Protocol.Validators;

using Domain.Protocol.Payloads;

public static class TxRemoveInputValidator
{
    public static void Validate(bool isInitiator, TxRemoveInputPayload input, Func<ulong, bool> isSerialIdPresent)
    {
        if (isInitiator && (input.SerialId & 1) != 0) // Ensure even serial_id for initiator
        {
            throw new InvalidOperationException("SerialId has the wrong parity.");
        }

        if (!isSerialIdPresent(input.SerialId))
        {
            throw new InvalidOperationException("The serial_id does not correspond to a currently added input.");
        }
    }
}