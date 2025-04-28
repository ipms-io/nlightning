namespace NLightning.Common.Interfaces;

using Messages.Payloads;

public interface IInteractiveTransactionService
{
    void AddInput(TxAddInputPayload input);
    void AddOutput(TxAddOutputPayload output);
    void RemoveInput(TxRemoveInputPayload input);
    void RemoveOutput(TxRemoveOutputPayload output);
}