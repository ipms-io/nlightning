namespace NLightning.NLTG.Interfaces;

public interface ITcpListenerService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
}