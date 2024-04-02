using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT1.Messages;

using Bolts.Base;
using Bolts.Exceptions;
using Constants;
using Payloads;

public sealed class PongMessage(ushort bytesLen) : BaseMessage(MessageTypes.PONG, new PongPayload(bytesLen))
{
    public new PongPayload Payload
    {
        get => (PongPayload)base.Payload;
        private set => base.Payload = value;
    }

    /// <summary>
    /// Deserialize a PongMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized PongMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing PongMessage</exception>
    public static async Task<PongMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            var payload = await PongPayload.DeserializeAsync(stream);

            return new PongMessage(payload.BytesLength)
            {
                Payload = payload
            };
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing PongMessage", e);
        }
    }
}