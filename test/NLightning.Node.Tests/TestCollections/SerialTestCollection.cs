namespace NLightning.Node.Tests.TestCollections;

[CollectionDefinition(Name, DisableParallelization = true)]
public class SerialTestCollection
{
    public const string Name = "serial";
}