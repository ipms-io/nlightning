using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT1.Payloads;

using Bolts.BOLT9;
using Bolts.Interfaces;

public class InitPayload(Features features) : IMessagePayload
{
    public Features Features { get; } = features;

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
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<InitPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var globalFeatures = await Features.DeserializeAsync(stream, true);

            var features = await Features.DeserializeAsync(stream);

            return new InitPayload(Features.Combine(globalFeatures, features));
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing InitPayload", e);
        }
    }
}