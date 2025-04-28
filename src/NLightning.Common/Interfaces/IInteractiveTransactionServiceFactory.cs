namespace NLightning.Common.Interfaces;

public interface IInteractiveTransactionServiceFactory
{
    IInteractiveTransactionService CreateInteractiveTransactionService(bool isInitiator);
}