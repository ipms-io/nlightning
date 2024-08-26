using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace NLightning.NLTG;

using Parsers;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Set up the DI container
            var services = new ServiceCollection();

            // Configure logging to use Serilog
            services.AddLogging(builder => builder.AddSerilog());

            // Initialize the options and logger from provided args
            var (options, loggerConfig) = ArgumentOptionParser.Initialize(args);

            // Check if the configuration file exists
            if (File.Exists(options.ConfigFile))
            {
                // Parse the configuration file
                (options, loggerConfig) = FileOptionParser.MergeWithFile(options.ConfigFile, options, loggerConfig);
            }
            else if (options.IsConfigFileDefault) // Create config with passed args only if it's in the default location
            {
                options.SaveToFile();
            }
            else
            {
                throw new Exception($"Config file not found: {options.ConfigFile}");
            }

            Log.Logger = loggerConfig.CreateLogger();

            Log.Logger.Information("Using config file: {ConfigFile}", options.ConfigFile);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            Log.Logger.Information("Config file .");
            Log.Logger.Debug("Log file: {LogFile}", options.LogFile);
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR: {e.Message}");
        }
    }
}