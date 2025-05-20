namespace NLightning.Infrastructure.Protocol.Interfaces;

using Domain.Protocol.Payloads;

public interface IInteractiveTransactionService
{
    void AddInput(TxAddInputPayload input);
    void AddOutput(TxAddOutputPayload output);
    void RemoveInput(TxRemoveInputPayload input);
    void RemoveOutput(TxRemoveOutputPayload output);
}