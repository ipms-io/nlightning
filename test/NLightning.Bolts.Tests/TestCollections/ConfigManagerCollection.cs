namespace NLightning.Bolts.Tests.TestCollections;

using Fixtures;

[CollectionDefinition(NAME)]
public class ConfigManagerCollection : ICollectionFixture<ConfigManagerFixture>
{
    public const string NAME = "config-manager";
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}