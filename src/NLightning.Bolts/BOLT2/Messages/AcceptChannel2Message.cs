using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents an open_channel2 message.
/// </summary>
/// <remarks>
/// The accept_channel2 message is sent to the initiator in order to accept the channel opening.
/// The message type is 65.
/// </remarks>
/// <param name="payload"></param>
public sealed class AcceptChannel2Message(AcceptChannel2Payload payload) : BaseMessage(MessageTypes.ACCEPT_CHANNEL_2, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new AcceptChannel2Payload Payload { get => (AcceptChannel2Payload)base.Payload; }

    /// <summary>
    /// Deserialize a OpenChannel2Message from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized OpenChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing OpenChannel2Message</exception>
    public static async Task<AcceptChannel2Message> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await AcceptChannel2Payload.DeserializeAsync(stream);

            return new AcceptChannel2Message(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing OpenChannel2Message", e);
        }
    }
}