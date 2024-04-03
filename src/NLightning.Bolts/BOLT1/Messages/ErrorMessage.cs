using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT1.Messages;

using Bolts.Base;
using Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents an error message.
/// </summary>
/// <remarks>
/// An error message is used to communicate an error to the other party.
/// The message type is 17.
/// </remarks>
/// <param name="payload">The error payload.</param>
public sealed class ErrorMessage(ErrorPayload payload) : BaseMessage(MessageTypes.ERROR, payload)
{
    /// <inheritdoc/>
    public new ErrorPayload Payload { get => (ErrorPayload)base.Payload; }

    /// <summary>
    /// Deserialize an ErrorMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized ErrorMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing ErrorMessage</exception>
    public static async Task<ErrorMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await ErrorPayload.DeserializeAsync(stream);

            return new ErrorMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ErrorMessage", e);
        }
    }
}