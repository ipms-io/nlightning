namespace NLightning.Domain.Protocol.Factories;

using Services;

/// <summary>
/// Interface for a ping pong service factory.
/// </summary>
/// <remarks>
/// This interface is used to create a ping pong service in test environments.
/// </remarks>
public interface IPingPongServiceFactory
{
    /// <summary>
    /// Creates a ping pong service.
    /// </summary>
    IPingPongService CreatePingPongService();
}