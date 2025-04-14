using CommandLine;
using CommandLine.Text;
using Serilog;

namespace NLightning.NLTG.Parsers;

using Common.Types;

public static class ArgumentOptionParser
{
    public static (Options, LoggerConfiguration) Initialize(string[] args)
    {
        LoggerConfiguration loggerConfiguration = new();
        Options opts = new();

        var parser = new Parser(with => with.HelpWriter = null);
        var parserResult = parser.ParseArguments<Args>(args);

        var helpText = HelpText.AutoBuild(parserResult, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = "NLightning Daemon";
            h.Copyright = "2024 IPMS";

            return HelpText.DefaultParsingErrorsHandler(parserResult, h);
        }, e => e);

        parserResult.WithParsed(parsedArgs =>
        {
            if (!parsedArgs.MainNet)
            {
                if (parsedArgs.TestNet)
                {
                    opts.Network = Network.TEST_NET;
                }
                else if (parsedArgs.RegTest)
                {
                    opts.Network = Network.REG_TEST;
                }
                else if (parsedArgs.SigNet)
                {
                    opts.Network = Network.SIG_NET;
                }
            }

            if (!string.IsNullOrWhiteSpace(parsedArgs.LogFile))
            {
                opts.LogFile = parsedArgs.LogFile;
            }

            if (!string.IsNullOrWhiteSpace(parsedArgs.LogLevel))
            {
                opts.LogLevel = parsedArgs.LogLevel.ToLower() switch
                {
                    "debug" => Serilog.Events.LogEventLevel.Debug,
                    "info" => Serilog.Events.LogEventLevel.Information,
                    "warning" => Serilog.Events.LogEventLevel.Warning,
                    "error" => Serilog.Events.LogEventLevel.Error,
                    "critical" => Serilog.Events.LogEventLevel.Fatal,
                    _ => throw new ArgumentException("Invalid log level.")
                };
            }

            opts.Daemon = parsedArgs.Daemon;
        });

        parserResult.WithNotParsed(errors =>
        {
            foreach (var error in errors)
            {
                if (error is HelpRequestedError or VersionRequestedError)
                {
                    Console.WriteLine();
                }
            }

            Environment.Exit(1);
        });

        // Check if the configuration file exists
        if (File.Exists(opts.ConfigFile))
        {
            // Parse the configuration file
            var fileOpts = FileOptionParser.GetOptionsFromFile(opts.ConfigFile);

            // Merge the configuration file options with the command line options
            // Command Line Options take precedence
            opts = opts.MergeWith(fileOpts);
        }
        else if (opts.IsConfigFileDefault) // Create config with passed args only if it's in the default location
        {
            opts.SaveToFile();
        }
        else
        {
            throw new Exception($"Config file not found: {opts.ConfigFile}");
        }

        loggerConfiguration.WriteTo.File(opts.LogFile);
        loggerConfiguration.MinimumLevel.Is(opts.LogLevel);

        if (!opts.Daemon)
        {
            loggerConfiguration.WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Sixteen);
        }

        return (opts, loggerConfiguration);
    }
}