namespace NLightning.Bolts.BOLT1.Factories;

using Interfaces;
using Services;

/// <summary>
/// Factory for creating a ping pong service.
/// </summary>
/// <remarks>
/// This class is used to create a ping pong service in test environments.
/// </remarks>
internal class PingPongServiceFactory : IPingPongServiceFactory
{
    /// <inheritdoc />
    public IPingPongService CreatePingPongService(TimeSpan networkTimeout)
    {
        return new PingPongService(networkTimeout);
    }
}