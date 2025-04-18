using System.Runtime.Serialization;

namespace NLightning.Common.Messages;

using Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a ping message.
/// </summary>
/// <remarks>
/// The ping message is used to check if the other party is still alive.
/// The message type is 18.
/// </remarks>
public sealed class PingMessage() : BaseMessage(MessageTypes.PING, new PingPayload())
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new PingPayload Payload => (PingPayload)base.Payload;

    internal PingMessage(PingPayload payload) : this()
    {
        base.Payload = payload;
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

            return new PingMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing PingMessage", e);
        }
    }
}