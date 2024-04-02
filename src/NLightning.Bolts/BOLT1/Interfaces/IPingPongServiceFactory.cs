namespace NLightning.Bolts.BOLT1.Interfaces;

public interface IPingPongServiceFactory
{
    IPingPongService CreatePingPongService(TimeSpan networkTimeout);
}