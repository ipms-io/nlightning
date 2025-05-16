namespace NLightning.Infrastructure.Serialization.Interfaces;

using Domain.Node;

public interface IFeatureSetSerializer
{
    Task SerializeAsync(FeatureSet featureSet, Stream stream, bool asGlobal = false, bool includeLength = true);
    Task<FeatureSet> DeserializeAsync(Stream stream, bool includeLength = true);
}