namespace NLightning.Bolts.Tests.TestCollections;

using Fixtures;

[CollectionDefinition(NAME)]
public class SecureKeyManagerCollection : ICollectionFixture<SecureKeyManagerFixture>
{
    public const string NAME = "secure-key-manager";
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}