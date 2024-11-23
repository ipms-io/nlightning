namespace NLightning.Bolts.BOLT1.Payloads;

using BOLT9;
using Bolts.Interfaces;
using Exceptions;

/// <summary>
/// The init payload.
/// </summary>
/// <remarks>
/// The init payload is used to communicate the features supported by the node.
/// </remarks>
/// <param name="features">The features supported by the node.</param>
/// <seealso cref="Messages.InitMessage"/>
/// <seealso cref="BOLT9.Features"/>
public class InitPayload(Features features) : IMessagePayload
{
    /// <summary>
    /// The features supported by the node.
    /// </summary>
    public Features Features { get; } = features;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await Features.SerializeAsync(stream, true);
        await Features.SerializeAsync(stream);
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
            var globalFeatures = await Features.DeserializeAsync(stream);

            var features = await Features.DeserializeAsync(stream);

            return new InitPayload(Features.Combine(globalFeatures, features));
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing InitPayload", e);
        }
    }
}