using Microsoft.Extensions.Hosting;
using NLightning.NLTG.Extensions;
using NLightning.NLTG.Helpers;
using NLightning.NLTG.Utilities;
using Serilog;

try
{
    // Bootstrap logger for startup messages
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

    // Get network for PID file path
    var network = CommandLineHelper.GetNetwork(args);
    var pidFilePath = DaemonUtility.GetPidFilePath(network);

    // Check for stop command
    if (CommandLineHelper.IsStopRequested(args))
    {
        var stopped = DaemonUtility.StopDaemon(pidFilePath, Log.Logger);
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

    // Read configuration file to check for daemon setting
    var initialConfig = NltgConfigurationExtensions.ReadInitialConfiguration(args);

    // Start as daemon if requested
    if (DaemonUtility.StartDaemonIfRequested(args, initialConfig, pidFilePath, Log.Logger))
    {
        // Parent process exits immediately after starting daemon
        return 0;
    }

    Log.Information("Starting NLTG...");

    // Create and run host
    await Host.CreateDefaultBuilder(args)
        .ConfigureNltg(initialConfig)
        .ConfigureNltgServices()
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