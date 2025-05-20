namespace NLightning.Infrastructure.Protocol.Interfaces;

public interface IInteractiveTransactionServiceFactory
{
    IInteractiveTransactionService CreateInteractiveTransactionService(bool isInitiator);
}