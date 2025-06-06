namespace NLightning.Integration.Tests.TestCollections;

using Fixtures;

[CollectionDefinition(Name)]
public class LightningRegtestNetworkFixtureCollection : ICollectionFixture<LightningRegtestNetworkFixture>
{
    public const string Name = "regtest";
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}