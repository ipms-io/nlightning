using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using NLightning.Bolts.BOLT1.Factories;
using NLightning.Bolts.BOLT1.Interfaces;
using NLightning.Bolts.BOLT1.Managers;
using NLightning.Common.Interfaces;
using NLightning.Common.Managers;
using NLightning.NLTG;
using NLightning.NLTG.Parsers;
using Serilog;
using ILogger = NLightning.Common.Interfaces.ILogger;

// Register signal handlers
var cts = new CancellationTokenSource();
PosixSignalRegistration.Create(PosixSignal.SIGTERM, HandleTermination);
PosixSignalRegistration.Create(PosixSignal.SIGQUIT, HandleTermination);

// Set up the DI container
var services = new ServiceCollection();

// Configure logging to use Serilog
services.AddLogging(builder => builder.AddSerilog());

// Initialize the options and logger from provided args
var (options, loggerConfig) = ArgumentOptionParser.Initialize(args);

Log.Logger = loggerConfig.CreateLogger();

Log.Logger.Information("Using config file: {ConfigFile}", options.ConfigFile);

Log.Logger.Information("Log file: {LogFile}", options.LogFile);
// Initialize ConfigManager
ConfigManager.NodeOptions = options.ToNodeOptions();

// Register services
services.AddSingleton<ILogger, SerilogLogger>();
services.AddSingleton<IPeerManager, PeerManager>();
services.AddScoped<ITransportServiceFactory, TransportServiceFactory>();
services.AddScoped<IMessageServiceFactory, MessageServiceFactory>();
services.AddScoped<IPingPongServiceFactory, PingPongServiceFactory>();

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

// Initialize Peer Manager
var peerManager = serviceProvider.GetRequiredService<IPeerManager>();

var listener = new TcpListener(IPAddress.Any, options.Port);
listener.Start();
Log.Logger.Information("Listening for connections on port {port}...", options.Port);
var key = new Key();
SecureKeyManager.Initialize(key.ToBytes());
Log.Logger.Debug("lncli connect {pubKey}@docker.for.mac.host.internal:9735", key.PubKey.ToString());

while (!cts.Token.IsCancellationRequested)
{
    var tcpClient = await listener.AcceptTcpClientAsync();
    _ = Task.Run(async () =>
    {
        try
        {
            Log.Logger.Information("[NLTG] New peer connection from {remoteEndPoint}", tcpClient.Client.RemoteEndPoint);
            await peerManager.AcceptPeerAsync(tcpClient);
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "[NLTG] Error accepting peer connection for {remoteEndPoint}",
                                tcpClient.Client.RemoteEndPoint);
        }
    }, cts.Token);
}

Log.Logger.Information("Shutting down...");

return;

void HandleTermination(PosixSignalContext context)
{
    Log.Logger.Information("Received {signal} signal, starting graceful shutdown...", context.Signal);
    context.Cancel = true;
    cts.Cancel();
}