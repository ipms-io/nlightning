using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT1.Messages;

using Base;
using Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents an init message.
/// </summary>
/// <remarks>
/// The init message is used to communicate the features of the node.
/// The message type is 16.
/// </remarks>
/// <param name="payload">The init payload.</param>
/// <param name="extension">The TLV extension.</param>
public sealed class InitMessage(InitPayload payload, TlvStream? extension = null) : BaseMessage(MessageTypes.INIT, payload, extension)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
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
            var extension = await TlvStream.DeserializeAsync(stream);

            return new InitMessage(payload, extension);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing InitMessage", e);
        }
    }
}