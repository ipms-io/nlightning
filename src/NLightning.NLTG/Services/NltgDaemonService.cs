using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLightning.Application.Interfaces.Services;
using NLightning.Application.Options;
using NLightning.Domain.Serialization;
using NLightning.Domain.ValueObjects;

namespace NLightning.NLTG.Services;

using Common.Interfaces;
using Common.Options;
using Interfaces;

public class NltgDaemonService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IFeeService _feeService;
    private readonly ILogger<NltgDaemonService> _logger;
    private readonly NodeOptions _nodeOptions;
    private readonly ISecureKeyManager _secureKeyManager;
    private readonly ITcpListenerService _tcpListenerService;

    public NltgDaemonService(IConfiguration configuration, IEndianConverter endianConverter, IFeeService feeService, ILogger<NltgDaemonService> logger,
                             IOptions<NodeOptions> nodeOptions, ISecureKeyManager secureKeyManager,
                             ITcpListenerService tcpListenerService)
    {
        _configuration = configuration;
        _feeService = feeService;
        _logger = logger;
        _nodeOptions = nodeOptions.Value;
        _secureKeyManager = secureKeyManager;
        _tcpListenerService = tcpListenerService;
        
        BigSize.SetEndianConverter(endianConverter);
        Witness.SetEndianConverter(endianConverter);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var network = _configuration["network"] ?? _configuration["n"] ?? _nodeOptions.Network;
        var isDaemon = _configuration.GetValue<bool?>("daemon") ?? _configuration.GetValue<bool?>("daemon-child") ?? _nodeOptions.Daemon;

        _logger.LogInformation("NLTG Daemon started on {Network} network", network);
        _logger.LogDebug("Running in daemon mode: {IsDaemon}", isDaemon);

        var pubKey = _secureKeyManager.GetNodePubKey();
        _logger.LogDebug("lncli connect {pubKey}@docker.for.mac.host.internal:9735", pubKey.ToString());

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