using Microsoft.Extensions.Hosting;
using NBitcoin;
using NLightning.Node.Extensions;
using NLightning.Node.Helpers;
using NLightning.Node.Managers;
using NLightning.Node.Utilities;
using Serilog;
using Network = NLightning.Domain.ValueObjects.Network;

try
{
    // Bootstrap logger for startup messages
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

    // Get network for the PID file path
    var network = CommandLineHelper.GetNetwork(args);
    var pidFilePath = DaemonUtils.GetPidFilePath(network);

    // Check for the stop command
    if (CommandLineHelper.IsStopRequested(args))
    {
        var stopped = DaemonUtils.StopDaemon(pidFilePath, Log.Logger);
        return stopped ? 0 : 1;
    }

    // Check for status command
    if (CommandLineHelper.IsStatusRequested(args))
    {
        ReportDaemonStatus(pidFilePath);
        return 0;
    }

    // Check if help is requested
    if (CommandLineHelper.IsHelpRequested(args))
    {
        CommandLineHelper.ShowUsage();
        return 0;
    }

    // Read the configuration file to check for daemon setting
    var initialConfig = NodeConfigurationExtensions.ReadInitialConfiguration(args);

    string? password = null;

    // Try to get password from args or prompt
    if (args.Contains("--password"))
    {
        var idx = Array.IndexOf(args, "--password");
        if (idx >= 0 && idx + 1 < args.Length)
            password = args[idx + 1];
    }
    if (string.IsNullOrWhiteSpace(password))
    {
        password = ConsoleUtils.ReadPassword("Enter password for key encryption: ");
    }
    if (string.IsNullOrWhiteSpace(password))
    {
        Log.Error("Password cannot be empty.");
        return 1;
    }

    SecureKeyManager keyManager;
    var keyFilePath = SecureKeyManager.GetKeyFilePath(network);
    if (!File.Exists(keyFilePath))
    {
        // Creates new key
        var key = new Key();
        keyManager = new SecureKeyManager(key.ToBytes(), new Network(network), keyFilePath);
        keyManager.SaveToFile(password);
        Console.WriteLine($"New key created and saved to {keyFilePath}");
    }
    else
    {
        // Load the existing key
        keyManager = SecureKeyManager.FromFilePath(keyFilePath, new Network(network), password);
        Console.WriteLine($"Loaded key from {keyFilePath}");
    }

    // Start as a daemon if requested
    if (DaemonUtils.StartDaemonIfRequested(args, initialConfig, pidFilePath, Log.Logger))
    {
        // The parent process exits immediately after starting the daemon
        return 0;
    }

    Log.Information("Starting NLTG...");

    // Create and run host
    await Host.CreateDefaultBuilder(args)
        .ConfigureNltg(initialConfig)
        .ConfigureNltgServices(keyManager)
        .RunConsoleAsync();

    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

static void ReportDaemonStatus(string pidFilePath)
{
    try
    {
        if (!File.Exists(pidFilePath))
        {
            Console.WriteLine("NLTG daemon is not running");
            return;
        }

        var pidText = File.ReadAllText(pidFilePath).Trim();
        if (!int.TryParse(pidText, out var pid))
        {
            Console.WriteLine("Invalid PID in file, daemon may not be running");
            return;
        }

        try
        {
            var process = System.Diagnostics.Process.GetProcessById(pid);
            var runTime = DateTime.Now - process.StartTime;

            Console.WriteLine("NLTG daemon is running");
            Console.WriteLine($"PID: {pid}");
            Console.WriteLine($"Started: {process.StartTime}");
            Console.WriteLine($"Uptime: {runTime.Days}d {runTime.Hours}h {runTime.Minutes}m");
            Console.WriteLine($"Memory: {process.WorkingSet64 / (1024 * 1024)} MB");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("NLTG daemon is not running (stale PID file)");
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error checking daemon status: {e.Message}");
    }
}