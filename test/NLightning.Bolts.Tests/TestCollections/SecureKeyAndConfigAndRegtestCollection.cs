namespace NLightning.Bolts.Tests.TestCollections;

using Fixtures;

[CollectionDefinition(NAME)]
public class SecureKeyAndConfigAndRegtestCollection : ICollectionFixture<SecureKeyManagerFixture>,
                                                      ICollectionFixture<LightningRegtestNetworkFixture>,
                                                      ICollectionFixture<ConfigManagerFixture>
{
    public const string NAME = "secure-key&config&regtest";
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}