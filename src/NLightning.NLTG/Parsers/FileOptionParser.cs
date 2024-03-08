using Serilog;

namespace NLightning.NLTG.Parsers;

public static class FileOptionParser
{
    public static (Options, LoggerConfiguration) MergeWithFile(string configFile, Options options, LoggerConfiguration loggerConfig)
    {
        var configFileOptions = Options.FromFile(configFile);

        // Merge the configuration file options with the command line options
        // Command Line Options take precedence
        var merged = options.MergeWith(configFileOptions);

        // Adjust where the log file is written to, if needed
        if (!merged.IsLogFileDefault)
        {
            loggerConfig.WriteTo.File(merged.LogFile);
        }

        return (merged, loggerConfig);
    }
}