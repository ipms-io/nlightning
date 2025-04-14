using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Common.Constants;
using Payloads;

/// <summary>
/// Represents a revoke_and_ack message.
/// </summary>
/// <remarks>
/// The revoke_and_ack message is used as a reply to the commitment_signed message.
/// The message type is 133.
/// </remarks>
/// <param name="payload"></param>
public sealed class RevokeAndAckMessage(RevokeAndAckPayload payload) : BaseMessage(MessageTypes.REVOKE_AND_ACK, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new RevokeAndAckPayload Payload { get => (RevokeAndAckPayload)base.Payload; }

    /// <summary>
    /// Deserialize a RevokeAndAckMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized RevokeAndAckMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing RevokeAndAckMessage</exception>
    public static async Task<RevokeAndAckMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await RevokeAndAckPayload.DeserializeAsync(stream);

            return new RevokeAndAckMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing RevokeAndAckMessage", e);
        }
    }
}