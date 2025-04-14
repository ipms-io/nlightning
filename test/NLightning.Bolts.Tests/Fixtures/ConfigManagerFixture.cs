namespace NLightning.Bolts.Tests.Fixtures;

using Common.Managers;
using Common.Node;

// ReSharper disable once ClassNeverInstantiated.Global
public class ConfigManagerFixture
{
    public ConfigManagerFixture()
    {
        ConfigManager.NodeOptions = new NodeOptions();
    }
}