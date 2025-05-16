using Microsoft.Extensions.Options;
using NLightning.Domain.Node;
using NLightning.Infrastructure.Protocol.Interfaces;

namespace NLightning.Infrastructure.Protocol.Factories;

using Common.Interfaces;
using Domain.Protocol.Interfaces;
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