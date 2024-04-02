namespace NLightning.Bolts.BOLT1.Messages;

using System.Runtime.Serialization;
using Bolts.Base;
using Bolts.Exceptions;
using Constants;
using Payloads;

public sealed class InitMessage(InitPayload payload, TLVStream? extension = null) : BaseMessage(MessageTypes.INIT, payload, extension)
{
    public new InitPayload Payload { get => (InitPayload)base.Payload; }

    /// <summary>
    /// Deserialize an InitMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized InitMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing InitMessage</exception>
    public static async Task<InitMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await InitPayload.DeserializeAsync(stream);

            // Deserialize extension if available
            var extension = await TLVStream.DeserializeAsync(stream);

            return new InitMessage(payload, extension);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ErrorMessage", e);
        }
    }
}