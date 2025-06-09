namespace NLightning.Infrastructure.Transport.Interfaces;

public interface ITcpListenerService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
}