namespace NLightning.Blazor.Tests.TestCollections;

using Infrastructure;

[CollectionDefinition("Blazor Test Collection", DisableParallelization = true)]
public class BlazorTestCollection : ICollectionFixture<BlazorTestBase>
{ }