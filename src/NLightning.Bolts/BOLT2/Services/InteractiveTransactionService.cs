namespace NLightning.Bolts.BOLT2.Services;

using Payloads;
using Validators;

public class InteractiveTransactionService
{
    private readonly List<TxAddInputPayload> _inputs = [];

    public void AddInput(TxAddInputPayload input)
    {
        TxAddInputValidator.Validate(input, _inputs.Count, IsValidPrevTx, IsUniqueInput, IsSerialIdUnique);
        _inputs.Add(input);
    }

    private Task<bool> IsValidPrevTx(byte[] prevTx)
    {
        // TODO: Implement logic to validate the prevTx by talking to bitcoind
        return Task.FromResult(true);
    }

    private bool IsUniqueInput(byte[] prevTx, uint prevTxVout)
    {
        return !_inputs.Any(i => i.PrevTx.SequenceEqual(prevTx) && i.PrevTxVout == prevTxVout);
    }

    private bool IsSerialIdUnique(ulong serialId)
    {
        return !_inputs.Any(i => i.SerialId == serialId);
    }
}