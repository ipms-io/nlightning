namespace NLightning.Bolts.BOLT2.Services;

using Payloads;
using Validators;

public class InteractiveTransactionService(bool isInitiator, ulong dustLimit)
{
    private readonly List<TxAddInputPayload> _inputs = [];
    private readonly List<TxAddOutputPayload> _outputs = [];
    private readonly bool _isInitiator = isInitiator;
    private readonly ulong _dustLimit = dustLimit;

    public void AddInput(TxAddInputPayload input)
    {
        TxAddInputValidator.Validate(_isInitiator, input, _inputs.Count, IsValidPrevTx, IsUniqueInput, IsSerialIdUnique);
        _inputs.Add(input);
    }

    public void AddOutput(TxAddOutputPayload output)
    {
        TxAddOutputValidator.Validate(_isInitiator, output, _outputs.Count, IsSerialIdUnique, IsStandardScript, _dustLimit);
        _outputs.Add(output);
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

    private bool IsStandardScript(byte[] script)
    {
        // TODO: Check using NBitcoin
        return script.Length > 0;
    }
}