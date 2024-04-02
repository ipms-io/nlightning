using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT1.Messages;

using Bolts.Base;
using Bolts.Exceptions;
using Constants;
using Payloads;

public sealed class WarningMessage(ErrorPayload payload) : BaseMessage(MessageTypes.WARNING, payload)
{
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