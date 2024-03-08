using CommandLine;

namespace NLightning.NLTG;

public class Args
{
    [Option('a', "addpeer", Required = false, HelpText = "Add a peer to the list of peers.")]
    public string? AddPeer { get; set; }

    [Option("logfile", Required = false, HelpText = "Set the location of the log file.")]
    public string? LogFile { get; set; }

    [Option('l', "loglevel", Required = false, HelpText = "Set the log level. DEBUG | INFO | WARNING | ERROR | CRITICAL")]
    public string? LogLevel { get; set; }

    [Option("mainnet", Required = false, HelpText = "Use mainnet.")]
    public bool MainNet { get; set; } = true;

    [Option("testnet", Required = false, HelpText = "Use testnet.")]
    public bool TestNet { get; set; }

    [Option("regtest", Required = false, HelpText = "Use regtest.")]
    public bool RegTest { get; set; }

    [Option("signet", Required = false, HelpText = "Use signet.")]
    public bool SigNet { get; set; }

    [Option('c', "configfile", Required = false, HelpText = "Use a configuration file.")]
    public string? ConfigFile { get; set; }

    [Option('d', "daemon", Required = false, HelpText = "Run as a daemon.")]
    public bool Daemon { get; set; }
}