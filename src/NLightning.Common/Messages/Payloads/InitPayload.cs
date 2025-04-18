namespace NLightning.Common.Messages.Payloads;

using Exceptions;
using Interfaces;
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

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await FeatureSet.SerializeAsync(stream, true);
        await FeatureSet.SerializeAsync(stream);
    }

    /// <summary>
    /// Deserialize an InitPayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized InitPayload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<InitPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var globalFeatures = await FeatureSet.DeserializeAsync(stream);

            var features = await FeatureSet.DeserializeAsync(stream);

            return new InitPayload(FeatureSet.Combine(globalFeatures, features));
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing InitPayload", e);
        }
    }
}