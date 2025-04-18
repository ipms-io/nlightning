namespace NLightning.Bolts.BOLT1.Factories;

using Common.Interfaces;
using Services;

/// <summary>
/// Factory for creating a ping pong service.
/// </summary>
/// <remarks>
/// This class is used to create a ping pong service in test environments.
/// </remarks>
public class PingPongServiceFactory : IPingPongServiceFactory
{
    private readonly IMessageFactory _messageFactory;

    public PingPongServiceFactory(IMessageFactory messageFactory)
    {
        _messageFactory = messageFactory;
    }

    /// <inheritdoc />
    public IPingPongService CreatePingPongService()
    {
        return new PingPongService(_messageFactory);
    }
}