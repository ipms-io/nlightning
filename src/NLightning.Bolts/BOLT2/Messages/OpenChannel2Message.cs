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
/// The open_channel2 message is sent to another peer in order to start the channel negotiation.
/// The message type is 64.
/// </remarks>
/// <param name="payload"></param>
public sealed class OpenChannel2Message(OpenChannel2Payload payload, TlvStream? extension) : BaseMessage(MessageTypes.OPEN_CHANNEL_2, payload, extension)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new OpenChannel2Payload Payload { get => (OpenChannel2Payload)base.Payload; }

    /// <summary>
    /// Deserialize a OpenChannel2Message from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized OpenChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing OpenChannel2Message</exception>
    public static async Task<OpenChannel2Message> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await OpenChannel2Payload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);

            return new OpenChannel2Message(payload, extension);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing OpenChannel2Message", e);
        }
    }
}