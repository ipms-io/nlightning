using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace NLightning.NLTG.Services;

using Common.Interfaces;
using Common.Managers;
using Common.Options;
using Interfaces;

public class NltgDaemonService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IFeeService _feeService;
    private readonly ILogger<NltgDaemonService> _logger;
    private readonly NodeOptions _nodeOptions;
    private readonly ITcpListenerService _tcpListenerService;

    public NltgDaemonService(IConfiguration configuration, IFeeService feeService, ILogger<NltgDaemonService> logger,
                             IOptions<NodeOptions> nodeOptions, ITcpListenerService tcpListenerService)
    {
        _configuration = configuration;
        _feeService = feeService;
        _logger = logger;
        _nodeOptions = nodeOptions.Value;
        _tcpListenerService = tcpListenerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var network = _configuration["network"] ?? _configuration["n"] ?? _nodeOptions.Network;
        var isDaemon = _configuration.GetValue<bool?>("daemon") ?? _configuration.GetValue<bool?>("daemon-child") ?? _nodeOptions.Daemon;

        _logger.LogInformation("NLTG Daemon started on {Network} network", network);
        _logger.LogDebug("Running in daemon mode: {IsDaemon}", isDaemon);

        var key = new Key();
        SecureKeyManager.Initialize(key.ToBytes());
        _logger.LogDebug("lncli connect {pubKey}@docker.for.mac.host.internal:9735", key.PubKey.ToString());

        try
        {
            // Start the fee service
            await _feeService.StartAsync(stoppingToken);

            // Start listening for connections
            await _tcpListenerService.StartAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopping NLTG daemon service");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NLTG shutdown requested");

        await Task.WhenAll(_feeService.StopAsync(), _tcpListenerService.StopAsync(), base.StopAsync(cancellationToken));

        _logger.LogInformation("NLTG daemon service stopped");
    }
}