namespace NLightning.Bolts.Tests.TestCollections;

using Fixtures;

[CollectionDefinition(NAME)]
public class SecureKeyAndRegtestCollection : ICollectionFixture<SecureKeyManagerFixture>,
                                             ICollectionFixture<LightningRegtestNetworkFixture>
{
    public const string NAME = "secure-key&regtest";
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}