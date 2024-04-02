using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT1.Messages;

using Bolts.Base;
using Bolts.Exceptions;
using Constants;
using Payloads;

public sealed class PingMessage() : BaseMessage(MessageTypes.PING, new PingPayload())
{
    public new PingPayload Payload
    {
        get => (PingPayload)base.Payload;
        private set => base.Payload = value;
    }

    /// <summary>
    /// Deserialize a PingMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized PingMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing PingMessage</exception>
    public static async Task<PingMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            var payload = await PingPayload.DeserializeAsync(stream);

            return new PingMessage
            {
                Payload = payload
            };
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing PingMessage", e);
        }
    }
}