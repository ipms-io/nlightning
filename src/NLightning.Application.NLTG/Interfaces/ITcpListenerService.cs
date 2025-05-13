namespace NLightning.Application.NLTG.Interfaces;

public interface ITcpListenerService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
}