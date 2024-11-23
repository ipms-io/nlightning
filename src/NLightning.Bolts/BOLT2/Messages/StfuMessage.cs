using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents an stfu message.
/// </summary>
/// <remarks>
/// The stfu message means SomeThing Fundamental is Underway, so we kindly ask the other node to STFU because we have
/// something important to say
/// The message type is 2.
/// </remarks>
/// <param name="payload"></param>
public sealed class StfuMessage(StfuPayload payload) : BaseMessage(MessageTypes.STFU, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new StfuPayload Payload { get => (StfuPayload)base.Payload; }

    /// <summary>
    /// Deserialize a StfuMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized StfuMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing StfuMessage</exception>
    public static async Task<StfuMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await StfuPayload.DeserializeAsync(stream);

            return new StfuMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing StfuMessage", e);
        }
    }
}