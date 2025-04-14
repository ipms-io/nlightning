using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Common.Constants;
using Payloads;

/// <summary>
/// Represents an update_fail_malformed_htlc message.
/// </summary>
/// <remarks>
/// The update_fail_malformed_htlc message is sent to let the peer know that the htlc is malformed
/// The message type is 135.
/// </remarks>
/// <param name="payload"></param>
public sealed class UpdateFailMalformedHtlcMessage(UpdateFailMalformedHtlcPayload payload) : BaseMessage(MessageTypes.UPDATE_FAIL_MALFORMED_HTLC, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new UpdateFailMalformedHtlcPayload Payload { get => (UpdateFailMalformedHtlcPayload)base.Payload; }

    /// <summary>
    /// Deserialize a UpdateFailMalformedHtlcMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized UpdateFailMalformedHtlcMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing UpdateFailMalformedHtlcMessage</exception>
    public static async Task<UpdateFailMalformedHtlcMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await UpdateFailMalformedHtlcPayload.DeserializeAsync(stream);

            return new UpdateFailMalformedHtlcMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing UpdateFailMalformedHtlcMessage", e);
        }
    }
}