using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Common.Constants;
using Payloads;

/// <summary>
/// Represents an commitment_signed message.
/// </summary>
/// <remarks>
/// The commitment_signed message is sent when a node has changes to the remote commitment
/// The message type is 132.
/// </remarks>
/// <param name="payload"></param>
public sealed class CommitmentSignedMessage(CommitmentSignedPayload payload) : BaseMessage(MessageTypes.COMMITMENT_SIGNED, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new CommitmentSignedPayload Payload { get => (CommitmentSignedPayload)base.Payload; }

    /// <summary>
    /// Deserialize a CommitmentSignedMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized CommitmentSignedMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing CommitmentSignedMessage</exception>
    public static async Task<CommitmentSignedMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await CommitmentSignedPayload.DeserializeAsync(stream);

            return new CommitmentSignedMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing CommitmentSignedMessage", e);
        }
    }
}