using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a update_fail_htlc message.
/// </summary>
/// <remarks>
/// The update_fail_htlc message is sent to let the peer know that the htlc has failed
/// The message type is 131.
/// </remarks>
/// <param name="payload"></param>
public sealed class UpdateFailHtlcMessage(UpdateFailHtlcPayload payload) : BaseMessage(MessageTypes.UPDATE_FAIL_HTLC, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new UpdateFailHtlcPayload Payload { get => (UpdateFailHtlcPayload)base.Payload; }

    /// <summary>
    /// Deserialize a UpdateFailHtlcMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized UpdateFailHtlcMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing UpdateFailHtlcMessage</exception>
    public static async Task<UpdateFailHtlcMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await UpdateFailHtlcPayload.DeserializeAsync(stream);

            return new UpdateFailHtlcMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing UpdateFailHtlcMessage", e);
        }
    }
}