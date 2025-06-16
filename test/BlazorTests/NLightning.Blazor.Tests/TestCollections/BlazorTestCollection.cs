namespace NLightning.Blazor.Tests.TestCollections;

using Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public class BlazorTestCollection : ICollectionFixture<BlazorTestBase>
{
    internal const string Name = "Blazor Test Collection";
}