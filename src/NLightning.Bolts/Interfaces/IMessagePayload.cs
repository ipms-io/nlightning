namespace NLightning.Bolts.Interfaces;

/// <summary>
/// Interface for a message payload.
/// </summary>
public interface IMessagePayload
{
    /// <summary>
    /// Deserializes the message payload.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SerializeAsync(Stream stream);
}