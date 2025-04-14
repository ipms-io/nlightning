using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Common.Constants;
using Payloads;

/// <summary>
/// Represents a shutdown message.
/// </summary>
/// <remarks>
/// The shutdown message is sent by either node to initiate closing, along with the scriptpubkey it wants to be paid to.
/// The message type is 38.
/// </remarks>
/// <param name="payload"></param>
public sealed class ShutdownMessage(ShutdownPayload payload) : BaseMessage(MessageTypes.SHUTDOWN, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ShutdownPayload Payload { get => (ShutdownPayload)base.Payload; }

    /// <summary>
    /// Deserialize a ShutdownMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized ShutdownMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing ShutdownMessage</exception>
    public static async Task<ShutdownMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await ShutdownPayload.DeserializeAsync(stream);

            return new ShutdownMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ShutdownMessage", e);
        }
    }
}