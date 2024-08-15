namespace NLightning.Common.Managers;

using Types;

public class ConfigManager
{
    private static readonly Lazy<ConfigManager> s_instance = new(() => new ConfigManager());

    // Private constructor to prevent external instantiation
    private ConfigManager() { }

    // Accessor for the singleton instance
    public static ConfigManager Instance => s_instance.Value;

    // Properties for storing configuration values
    public Network Network { get; set; }
}