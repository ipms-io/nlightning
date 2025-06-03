namespace NLightning.Node.Interfaces;

public interface ITcpListenerService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
}