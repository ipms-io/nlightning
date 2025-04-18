using NLightning.Bolts.Tests.Fixtures;

namespace NLightning.Bolts.Tests.TestCollections;
[CollectionDefinition(NAME)]
public class LightningRegtestNetworkFixtureCollection : ICollectionFixture<LightningRegtestNetworkFixture>
{
    public const string NAME = "regtest";
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}