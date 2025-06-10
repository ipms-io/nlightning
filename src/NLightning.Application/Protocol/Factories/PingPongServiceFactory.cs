using Microsoft.Extensions.Options;

namespace NLightning.Application.Protocol.Factories;

using Domain.Node.Interfaces;
using Domain.Node.Options;
using Domain.Protocol.Interfaces;
using Node.Services;

/// <summary>
/// Factory for creating a ping pong service.
/// </summary>
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