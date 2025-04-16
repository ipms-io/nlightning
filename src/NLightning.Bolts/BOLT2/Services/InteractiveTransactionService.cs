namespace NLightning.Bolts.BOLT2.Services;

using Common.Managers;
using Common.Messages.Payloads;
using Validators;

public class InteractiveTransactionService(bool isInitiator)
{
    private readonly Dictionary<ulong, TxAddInputPayload> _inputs = [];
    private readonly Dictionary<ulong, TxAddOutputPayload> _outputs = [];

    public void AddInput(TxAddInputPayload input)
    {
        TxAddInputValidator.Validate(isInitiator, input, _inputs.Count, IsValidPrevTx, IsUniqueInput, IsSerialIdUnique);
        _inputs.Add(input.SerialId, input);
    }

    public void AddOutput(TxAddOutputPayload output)
    {
        TxAddOutputValidator.Validate(isInitiator, output, _outputs.Count, IsSerialIdUnique, IsStandardScript,
                                      ConfigManager.Instance.DustLimitAmount);
        _outputs.Add(output.SerialId, output);
    }

    public void RemoveInput(TxRemoveInputPayload input)
    {
        TxRemoveInputValidator.Validate(isInitiator, input, IsSerialIdPresent);
        _inputs.Remove(input.SerialId);
    }

    public void RemoveOutput(TxRemoveOutputPayload output)
    {
        TxRemoveOutputValidator.Validate(isInitiator, output, IsSerialIdPresent);
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