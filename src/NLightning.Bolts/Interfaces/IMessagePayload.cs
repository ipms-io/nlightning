namespace NLightning.Bolts.Interfaces;

/// <summary>
/// Interface for a message payload.
/// </summary>
public interface IMessagePayload
{
    /// <summary>
    /// Serializes the message payload to a stream.
    /// </summary>
    /// <param name="stream">The stream to serialize to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SerializeAsync(Stream stream);
}