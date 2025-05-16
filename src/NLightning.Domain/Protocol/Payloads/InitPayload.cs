namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Messages;
using Node;

/// <summary>
/// The init payload.
/// </summary>
/// <remarks>
/// The init payload is used to communicate the features supported by the node.
/// </remarks>
/// <param name="featureSet">The features supported by the node.</param>
/// <seealso cref="InitMessage"/>
/// <seealso cref="FeatureSet"/>
public class InitPayload(FeatureSet featureSet) : IMessagePayload
{
    /// <summary>
    /// The features supported by the node.
    /// </summary>
    public FeatureSet FeatureSet { get; } = featureSet;
}