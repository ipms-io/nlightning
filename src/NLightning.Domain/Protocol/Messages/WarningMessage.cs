using System.Runtime.Serialization;
using NLightning.Common.Messages;
using NLightning.Domain.Protocol.Payloads;

namespace NLightning.Domain.Protocol.Messages;

/// <summary>
/// Represents a warning message.
/// </summary>
/// <remarks>
/// A warning message is used to communicate a warning to the other party.
/// The message type is 1.
/// </remarks>
/// <param name="payload">The warning payload. <see cref="ErrorPayload"/></param>
public sealed class WarningMessage(ErrorPayload payload) : BaseMessage(MessageTypes.WARNING, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ErrorPayload Payload { get => (ErrorPayload)base.Payload; }

    /// <summary>
    /// Deserialize a WarningMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized WarningMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing WarningMessage</exception>
    public static async Task<WarningMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await ErrorPayload.DeserializeAsync(stream);

            return new WarningMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing WarningMessage", e);
        }
    }
}