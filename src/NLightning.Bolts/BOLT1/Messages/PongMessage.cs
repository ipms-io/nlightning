using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT1.Messages;

using Base;
using Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a pong message.
/// </summary>
/// <remarks>
/// The pong message is used to respond to a ping message.
/// The message type is 19.
/// </remarks>
/// <param name="bytesLen">The number of bytes in the pong message.</param>
public sealed class PongMessage(ushort bytesLen) : BaseMessage(MessageTypes.PONG, new PongPayload(bytesLen))
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new PongPayload Payload => (PongPayload)base.Payload;

    private PongMessage(PongPayload payload) : this(payload.BytesLength)
    {
        base.Payload = payload;
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

            return new PongMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing PongMessage", e);
        }
    }
}