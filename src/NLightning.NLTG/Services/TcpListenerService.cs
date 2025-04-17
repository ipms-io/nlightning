using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NLightning.NLTG.Services;

using Common.Interfaces;
using Interfaces;

public class TcpListenerService : ITcpListenerService
{
    private readonly ILogger<TcpListenerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IPeerManager _peerManager;
    private readonly List<TcpListener> _listeners = [];

    private CancellationTokenSource? _cts;
    private Task? _listeningTask;

    public TcpListenerService(ILogger<TcpListenerService> logger, IConfiguration configuration,
                              IPeerManager peerManager)
    {
        _logger = logger;
        _configuration = configuration;
        _peerManager = peerManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var listenAddresses = _configuration.GetSection("ListenAddress").Get<string[]>() ?? [];

        foreach (var address in listenAddresses)
        {
            var parts = address.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
            {
                _logger.LogWarning("Invalid listen address: {Address}", address);
                continue;
            }

            var ipAddress = IPAddress.Parse(parts[0]);
            var listener = new TcpListener(ipAddress, port);
            listener.Start();
            _listeners.Add(listener);

            _logger.LogInformation("Listening for connections on {Address}:{Port}", ipAddress, port);
        }

        _listeningTask = ListenForConnectionsAsync(_cts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_cts is null)
        {
            throw new InvalidOperationException("Service is not running");
        }

        await _cts.CancelAsync();

        foreach (var listener in _listeners)
        {
            try
            {
                listener.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping listener");
            }
        }

        _listeners.Clear();

        if (_listeningTask is not null)
        {
            try
            {
                await _listeningTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during cancellation
            }
        }
    }

    private async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var listener in _listeners)
                {
                    if (!listener.Pending())
                    {
                        continue;
                    }

                    var tcpClient = await listener.AcceptTcpClientAsync(cancellationToken);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            _logger.LogInformation("New peer connection from {RemoteEndPoint}", tcpClient.Client.RemoteEndPoint);
                            await _peerManager.AcceptPeerAsync(tcpClient);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Error accepting peer connection for {RemoteEndPoint}",
                                tcpClient.Client.RemoteEndPoint);
                        }
                    }, cancellationToken);
                }

                await Task.Delay(100, cancellationToken); // Avoid busy-waiting
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Listener service shutdown requested");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception in listener service");
        }
    }
}