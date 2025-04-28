using Microsoft.Extensions.Options;

namespace NLightning.Bolts.BOLT2.Factories;

using Common.Interfaces;
using Common.Options;
using Services;

/// <summary>
/// Factory for creating a message service.
/// </summary>
/// <remarks>
/// This class is used to create a message service in test environments.
/// </remarks>
/// 
public class InteractiveTransactionServiceFactory : IInteractiveTransactionServiceFactory
{
    private readonly NodeOptions _nodeOptions;

    public InteractiveTransactionServiceFactory(IOptions<NodeOptions> nodeOptions)
    {
        _nodeOptions = nodeOptions.Value;
    }

    /// <inheritdoc />
    public IInteractiveTransactionService CreateInteractiveTransactionService(bool isInitiator)
    {
        return new InteractiveTransactionService(_nodeOptions.DustLimitAmount, isInitiator);
    }
}