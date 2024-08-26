using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace NLightning.NLTG;

using Common.Types;

[YamlSerializable]
public class Options
{
    /// <summary>
    /// 0: User Folder
    /// 1: Network
    /// </summary>
    private const string LOG_FILE = "{0}/.nltg/{1}/.log";

    /// <summary>
    /// 0: User Folder
    /// 1: Network
    /// </summary>
    private const string CONFIG_FILE = "{0}/.nltg/{1}/config.yaml";

    /// <summary>
    /// List of peers to connect to
    /// </summary>
    public List<object> Peers { get; set; }

    /// <summary>
    /// Log file location to write to
    /// </summary>
    public string LogFile { get; set; }

    /// <summary>
    /// Log level to write to the log file
    /// </summary>
    public Serilog.Events.LogEventLevel LogLevel { get; set; }

    /// <summary>
    /// Network to connect to
    /// </summary>
    /// <seealso cref="Network"/>
    public Network Network { get; set; }

    /// <summary>
    /// Path to the configuration file
    /// </summary>
    [YamlIgnore]
    public string ConfigFile { get; set; }

    /// <summary>
    /// Check if the config file is the default location
    /// </summary>
    /// <returns></returns>
    [YamlIgnore]
    public bool IsConfigFileDefault => ConfigFile == string.Format(CONFIG_FILE, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Network);

    /// <summary>
    /// Check if the log file is the default location
    /// </summary>
    /// <returns></returns>
    [YamlIgnore]
    public bool IsLogFileDefault => LogFile == string.Format(LOG_FILE, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Network);

    public Options()
    {
        Peers = [];
        Network = Network.MAIN_NET;
        LogFile = string.Format(LOG_FILE, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Network);
        LogLevel = Serilog.Events.LogEventLevel.Warning;
        ConfigFile = string.Format(CONFIG_FILE, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Network);
    }

    /// <summary>
    /// Replace the current network with the provided network
    /// </summary>
    /// <param name="network"></param>
    /// <returns></returns>
    public Options WithNetwork(Network network)
    {
        Network = network;

        // Update the log file and config file location if they are default
        if (IsLogFileDefault)
        {
            LogFile = string.Format(LOG_FILE, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Network);
        }

        if (IsConfigFileDefault)
        {
            ConfigFile = string.Format(CONFIG_FILE, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Network);
        }

        return this;
    }

    /// <summary>
    /// Load the Options from a file
    /// </summary>
    /// <param name="configFile">Config file location</param>
    /// <returns>Serialized Options</returns>
    public static Options FromFile(string configFile)
    {
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new NetworkConverter())
            .Build();
        var yamlString = File.ReadAllText(configFile);
        return deserializer.Deserialize<Options>(yamlString);
    }

    /// <summary>
    /// Save the Options to the current config file
    /// </summary>
    public void SaveToFile()
    {
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new NetworkConverter())
            .Build();
        var yaml = serializer.Serialize(this);
        File.WriteAllText(ConfigFile, yaml);
    }

    /// <summary>
    /// Merge this Options object with another Options object
    /// </summary>
    /// <remarks>
    /// This takes precedence over Other
    /// </remarks>
    /// <param name="other">Secondary Options to merge with</param>
    /// <returns>Merged Options</returns>
    public Options MergeWith(Options other)
    {
        var merged = new Options()
        {
            LogFile = !string.IsNullOrWhiteSpace(LogFile) ? LogFile : other.LogFile,
            LogLevel = LogLevel != Serilog.Events.LogEventLevel.Warning ? LogLevel : other.LogLevel,
            Network = Network != Network.MAIN_NET ? Network : other.Network,
            Peers = Peers
        };

        // Merge Peers and filter by unique
        merged.Peers.AddRange(other.Peers.Where(p => !Peers.Contains(p)));

        return merged;
    }

    /// <summary>
    /// Convert the Options to a string following the YAML format
    /// </summary>
    /// <returns>String that mimics the config file</returns>
    public override string ToString()
    {
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new NetworkConverter())
            .Build();
        return serializer.Serialize(this);
    }

    /// <summary>
    /// Convert the Network option to a simple string for serialization
    /// </summary>
    public class NetworkConverter : IYamlTypeConverter
    {
        /// <inheritdoc/>
        public bool Accepts(Type type)
        {
            return type == typeof(Network);
        }

        /// <inheritdoc/>
        public object ReadYaml(IParser parser, Type type)
        {
            var value = parser.Consume<Scalar>().Value;
            parser.MoveNext();
            return new Network(value);
        }

        /// <inheritdoc/>
        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value is Network network)
            {
                emitter.Emit(new Scalar(null, null, network.Name, ScalarStyle.Any, true, false));
            }
        }
    }
}