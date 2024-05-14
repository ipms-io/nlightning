namespace NLightning.Bolts.BOLT2.Services;

using Payloads;
using Validators;

public class InteractiveTransactionService(bool isInitiator, ulong dustLimit)
{
    private readonly Dictionary<ulong, TxAddInputPayload> _inputs = [];
    private readonly Dictionary<ulong, TxAddOutputPayload> _outputs = [];
    private readonly bool _isInitiator = isInitiator;
    private readonly ulong _dustLimit = dustLimit;

    public void AddInput(TxAddInputPayload input)
    {
        TxAddInputValidator.Validate(_isInitiator, input, _inputs.Count, IsValidPrevTx, IsUniqueInput, IsSerialIdUnique);
        _inputs.Add(input.SerialId, input);
    }

    public void AddOutput(TxAddOutputPayload output)
    {
        TxAddOutputValidator.Validate(_isInitiator, output, _outputs.Count, IsSerialIdUnique, IsStandardScript, _dustLimit);
        _outputs.Add(output.SerialId, output);
    }

    public void RemoveInput(TxRemoveInputPayload input)
    {
        TxRemoveInputValidator.Validate(_isInitiator, input, IsSerialIdPresent);
        _inputs.Remove(input.SerialId);
    }

    public void RemoveOutput(TxRemoveOutputPayload output)
    {
        TxRemoveOutputValidator.Validate(_isInitiator, output, IsSerialIdPresent);
        _outputs.Remove(output.SerialId);
    }

    private Task<bool> IsValidPrevTx(byte[] prevTx)
    {
        // TODO: Implement logic to validate the prevTx by talking to bitcoind
        return Task.FromResult(true);
    }

    private bool IsUniqueInput(byte[] prevTx, uint prevTxVout)
    {
        return !_inputs.Values.Any(i => i.PrevTx.SequenceEqual(prevTx) && i.PrevTxVout == prevTxVout);
    }

    private bool IsSerialIdUnique(ulong serialId)
    {
        return !_inputs.ContainsKey(serialId);
    }

    private bool IsSerialIdPresent(ulong serialId)
    {
        return _inputs.ContainsKey(serialId);
    }

    private bool IsStandardScript(byte[] script)
    {
        // TODO: Check using NBitcoin
        return script.Length > 0;
    }
}