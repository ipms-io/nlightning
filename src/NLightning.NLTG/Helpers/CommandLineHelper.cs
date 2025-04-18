namespace NLightning.NLTG.Helpers;

/// <summary>
/// Helper class for displaying command line usage information
/// </summary>
public static class CommandLineHelper
{
    public static void ShowUsage()
    {
        Console.WriteLine("NLTG - NLightning Daemon");
        Console.WriteLine("Usage:");
        Console.WriteLine("  nltg [options]");
        Console.WriteLine("  nltg --stop         Stop a running daemon");
        Console.WriteLine("  nltg --status       Show daemon status");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --network, -n <network>    Network to use (mainnet, testnet, regtest) [default: mainnet]");
        Console.WriteLine("  --config, -c <path>        Path to custom configuration file");
        Console.WriteLine("  --daemon <true|false>      Run as a daemon [default: false]");
        Console.WriteLine("  --stop                     Stop a running daemon");
        Console.WriteLine("  --status                   Show daemon status information");
        Console.WriteLine("  --help, -h, -?             Show this help message");
        Console.WriteLine();
        Console.WriteLine("Environment Variables:");
        Console.WriteLine("  NLTG_NETWORK               Network to use");
        Console.WriteLine("  NLTG_CONFIG                Path to custom configuration file");
        Console.WriteLine("  NLTG_DAEMON                Run as a daemon");
        Console.WriteLine();
        Console.WriteLine("Configuration File:");
        Console.WriteLine("  Default path: ~/.nltg/{network}/appsettings.json");
        Console.WriteLine("  Settings:");
        Console.WriteLine("  {");
        Console.WriteLine("    \"Daemon\": true,         # Run as a background daemon");
        Console.WriteLine("    ... other settings ...");
        Console.WriteLine("  }");
        Console.WriteLine();
        Console.WriteLine("PID file location: ~/.nltg/{network}/nltg.pid");
    }

    /// <summary>
    /// Parse command line arguments to check for help request
    /// </summary>
    public static bool IsHelpRequested(string[] args)
    {
        return args.Any(arg =>
            arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
            arg.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
            arg.Equals("--blorg", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsStopRequested(string[] args)
    {
        return args.Any(arg =>
            arg.Equals("--stop", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsStatusRequested(string[] args)
    {
        return args.Any(arg =>
            arg.Equals("--status", StringComparison.OrdinalIgnoreCase));
    }

    public static string GetNetwork(string[] args)
    {
        var network = "mainnet"; // Default

        // Check command line args
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("--network", StringComparison.OrdinalIgnoreCase) ||
                args[i].Equals("-n", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    network = args[i + 1];
                    break;
                }
            }

            if (!args[i].StartsWith("--network=", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            network = args[i]["--network=".Length..];
            break;
        }

        // Check environment variable if not found in args
        var envNetwork = Environment.GetEnvironmentVariable("NLTG_NETWORK");
        if (!string.IsNullOrEmpty(envNetwork))
        {
            network = envNetwork;
        }

        return network;
    }
}