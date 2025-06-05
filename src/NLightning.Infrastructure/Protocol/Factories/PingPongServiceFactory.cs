using Microsoft.Extensions.Options;
using NLightning.Application.Node.Interfaces;
using NLightning.Application.Node.Services;
using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Infrastructure.Protocol.Factories;

using Domain.Node.Options;
using Domain.Protocol.Services;

/// <summary>
/// Factory for creating a ping pong service.
/// </summary>
/// <remarks>
/// This class is used to create a ping pong service in test environments.
/// </remarks>
public class PingPongServiceFactory : IPingPongServiceFactory
{
    private readonly IMessageFactory _messageFactory;
    private readonly IOptions<NodeOptions> _nodeOptions;

    public PingPongServiceFactory(IMessageFactory messageFactory, IOptions<NodeOptions> nodeOptions)
    {
        _messageFactory = messageFactory;
        _nodeOptions = nodeOptions;
    }

    /// <inheritdoc />
    public IPingPongService CreatePingPongService()
    {
        return new PingPongService(_messageFactory, _nodeOptions);
    }
}