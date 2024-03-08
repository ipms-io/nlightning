using CommandLine;
using CommandLine.Text;
using Serilog;

namespace NLightning.NLTG.Parsers;

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
                    opts.Network = Network.TestNet;
                }
                else if (parsedArgs.RegTest)
                {
                    opts.Network = Network.RegTest;
                }
                else if (parsedArgs.SigNet)
                {
                    opts.Network = Network.SigNet;
                }
            }

            if (!string.IsNullOrWhiteSpace(parsedArgs.LogFile))
            {
                opts.LogFile = parsedArgs.LogFile;
            }
            loggerConfiguration.WriteTo.File(opts.LogFile);

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
            loggerConfiguration.MinimumLevel.Is(opts.LogLevel);

            if (!parsedArgs.Daemon)
            {
                loggerConfiguration.WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Sixteen);
            }
        });

        parserResult.WithNotParsed(errors =>
        {
            foreach (var error in errors)
            {
                if (error is HelpRequestedError || error is VersionRequestedError)
                {
                    Console.WriteLine();
                }
            }

            Environment.Exit(1);
        });

        return (opts, loggerConfiguration);
    }
}