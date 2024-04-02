namespace NLightning.Bolts.BOLT1.Factories;

using Interfaces;
using Services;

public class PingPongServiceFactory : IPingPongServiceFactory
{
    public IPingPongService CreatePingPongService(TimeSpan networkTimeout)
    {
        return new PingPongService(networkTimeout);
    }
}