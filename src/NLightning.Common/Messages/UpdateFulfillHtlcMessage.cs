using System.Runtime.Serialization;

namespace NLightning.Common.Messages;

using Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a update_fulfill_htlc message.
/// </summary>
/// <remarks>
/// The update_fulfill_htlc message is sent to let the peer know that the htlc was fulfiled
/// The message type is 130.
/// </remarks>
/// <param name="payload"></param>
public sealed class UpdateFulfillHtlcMessage(UpdateFulfillHtlcPayload payload) : BaseMessage(MessageTypes.UPDATE_FULFILL_HTLC, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new UpdateFulfillHtlcPayload Payload { get => (UpdateFulfillHtlcPayload)base.Payload; }

    /// <summary>
    /// Deserialize a UpdateFulfillHtlcMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized UpdateFulfillHtlcMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing UpdateFulfillHtlcMessage</exception>
    public static async Task<UpdateFulfillHtlcMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await UpdateFulfillHtlcPayload.DeserializeAsync(stream);

            return new UpdateFulfillHtlcMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing UpdateFulfillHtlcMessage", e);
        }
    }
}